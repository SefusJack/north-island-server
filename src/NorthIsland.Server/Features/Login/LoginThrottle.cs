using System.Collections.Concurrent;
using System.Net;
using Microsoft.Extensions.Options;

namespace NorthIsland.Server.Features.Login;

/// <summary>
/// Slows down password guessing and mass registration: too many failed logins of one account name or from one address lock further logins of
/// that name or address out for a while, and one address may auto-register only a few accounts per hour. Loopback addresses have no address
/// limits (the server's own test tools), but their logins still count for the account name.
/// </summary>
/// <param name="options">The server options: the failures before a lockout, the lockout time and the registrations per hour.</param>
public sealed class LoginThrottle(
    IOptions<ServerOptions> options)
{
    /// <summary>The milliseconds in a second.</summary>
    private const long MsPerSecond = 1000;

    /// <summary>The milliseconds of the registration window: one hour.</summary>
    private const long RegistrationWindowMs = 3600 * MsPerSecond;

    /// <summary>The calls between two sweeps of the entries whose window and lockout are over.</summary>
    private const int SweepEvery = 1024;

    /// <summary>The server options, read once at construction.</summary>
    private readonly ServerOptions _options = options.Value;

    /// <summary>The failed logins by account name, ignoring case.</summary>
    private readonly ConcurrentDictionary<string, Strikes> _byName = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>The failed logins by client address.</summary>
    private readonly ConcurrentDictionary<IPAddress, Strikes> _byAddress = new();

    /// <summary>The auto-registrations by client address.</summary>
    private readonly ConcurrentDictionary<IPAddress, Strikes> _registrations = new();

    /// <summary>The calls since the latest sweep; changed with Interlocked.</summary>
    private int _sinceSweep;

    /// <summary>The milliseconds of the failure window and of a lockout.</summary>
    private long LockoutMs => Math.Max(1, _options.LoginLockoutSeconds) * MsPerSecond;

    /// <summary>Gives the seconds left of a lockout of the account name or the address.</summary>
    /// <param name="name">The account name.</param>
    /// <param name="address">The client address, or null when it is not known.</param>
    /// <returns>The seconds until logins are let in again; 0 when they are not locked out.</returns>
    public int LockedSeconds(string name, IPAddress? address)
    {
        var now = Environment.TickCount64;
        var left = Math.Max(LeftMs(_byName, name, now), Limited(address) is { } a ? LeftMs(_byAddress, a, now) : 0);
        return (int)((left + MsPerSecond - 1) / MsPerSecond); // rounded up to whole seconds
    }

    /// <summary>Counts a failed login of an account name from an address, locking either out once it reaches the failures before a lockout.</summary>
    /// <param name="name">The account name.</param>
    /// <param name="address">The client address, or null when it is not known.</param>
    public void Failed(string name, IPAddress? address)
    {
        var now = Environment.TickCount64;
        var limit = Math.Max(1, _options.LoginFailuresBeforeLockout);
        _byName.GetOrAdd(name, static _ => new Strikes()).Count(now, LockoutMs, limit, LockoutMs);
        if (Limited(address) is { } a)
            _byAddress.GetOrAdd(a, static _ => new Strikes()).Count(now, LockoutMs, limit, LockoutMs);
        SweepNow(now);
    }

    /// <summary>Forgets the failed logins of an account name after a successful login.</summary>
    /// <param name="name">The account name.</param>
    public void Succeeded(string name) => _byName.TryRemove(name, out _);

    /// <summary>Counts an auto-registration from an address when the address has not used up its registrations of the past hour.</summary>
    /// <param name="address">The client address, or null when it is not known.</param>
    /// <returns>True when the address may register the account.</returns>
    public bool TryRegister(IPAddress? address)
    {
        if (_options.RegistrationsPerAddressPerHour <= 0 || Limited(address) is not { } a)
            return true;
        var now = Environment.TickCount64;
        SweepNow(now);
        return _registrations.GetOrAdd(a, static _ => new Strikes()).TryCount(now, RegistrationWindowMs, _options.RegistrationsPerAddressPerHour);
    }

    /// <summary>Gives the address the address limits apply to: null for an unknown or loopback address.</summary>
    /// <param name="address">The client address.</param>
    /// <returns>The address, IPv4 when it is an IPv4 address mapped to IPv6, or null.</returns>
    private static IPAddress? Limited(IPAddress? address) => address is null || IPAddress.IsLoopback(address) ? null
        : address.IsIPv4MappedToIPv6 ? address.MapToIPv4() : address;

    /// <summary>Gives the milliseconds left of the lockout of a key.</summary>
    /// <typeparam name="TKey">The key type.</typeparam>
    /// <param name="map">The strikes by key.</param>
    /// <param name="key">The key.</param>
    /// <param name="now">The current <see cref="Environment.TickCount64"/>.</param>
    /// <returns>The milliseconds left; 0 when the key is not locked out.</returns>
    private static long LeftMs<TKey>(ConcurrentDictionary<TKey, Strikes> map, TKey key, long now) where TKey : notnull =>
        map.TryGetValue(key, out var strikes) ? strikes.LockedMs(now) : 0;

    /// <summary>Every <see cref="SweepEvery"/> calls, removes the entries whose window and lockout are over, so that the maps stay small.</summary>
    /// <param name="now">The current <see cref="Environment.TickCount64"/>.</param>
    private void SweepNow(long now)
    {
        if (Interlocked.Increment(ref _sinceSweep) % SweepEvery != 0)
            return;
        Sweep(_byName, now, LockoutMs);
        Sweep(_byAddress, now, LockoutMs);
        Sweep(_registrations, now, RegistrationWindowMs);
    }

    /// <summary>Removes the entries of a map whose window and lockout are over.</summary>
    /// <typeparam name="TKey">The key type.</typeparam>
    /// <param name="map">The strikes by key.</param>
    /// <param name="now">The current <see cref="Environment.TickCount64"/>.</param>
    /// <param name="windowMs">The window of the map's entries in milliseconds.</param>
    private static void Sweep<TKey>(ConcurrentDictionary<TKey, Strikes> map, long now, long windowMs) where TKey : notnull
    {
        foreach (var (key, strikes) in map)
        {
            if (strikes.Expired(now, windowMs))
                map.TryRemove(new KeyValuePair<TKey, Strikes>(key, strikes));
        }
    }

    /// <summary>The counted events of one key in its current window, and its lockout.</summary>
    private sealed class Strikes
    {
        /// <summary>Held while the counts change.</summary>
        private readonly Lock _lock = new();

        /// <summary>When the current window began, in <see cref="Environment.TickCount64"/> milliseconds.</summary>
        private long _windowStartMs;

        /// <summary>The events counted in the current window.</summary>
        private int _count;

        /// <summary>Until when the key is locked out, in <see cref="Environment.TickCount64"/> milliseconds; 0 for no lockout.</summary>
        private long _lockedUntilMs;

        /// <summary>Counts an event and locks the key out once the window holds the limit of events.</summary>
        /// <param name="now">The current <see cref="Environment.TickCount64"/>.</param>
        /// <param name="windowMs">The window in milliseconds.</param>
        /// <param name="limit">The events that start a lockout.</param>
        /// <param name="lockoutMs">The lockout in milliseconds.</param>
        public void Count(long now, long windowMs, int limit, long lockoutMs)
        {
            lock (_lock)
            {
                StartWindow(now, windowMs);
                if (++_count < limit)
                    return;
                _count = 0;
                _lockedUntilMs = now + lockoutMs;
            }
        }

        /// <summary>Counts an event when the window does not yet hold the limit of events.</summary>
        /// <param name="now">The current <see cref="Environment.TickCount64"/>.</param>
        /// <param name="windowMs">The window in milliseconds.</param>
        /// <param name="limit">The events allowed per window.</param>
        /// <returns>True when the event was counted.</returns>
        public bool TryCount(long now, long windowMs, int limit)
        {
            lock (_lock)
            {
                StartWindow(now, windowMs);
                if (_count >= limit)
                    return false;
                _count++;
                return true;
            }
        }

        /// <summary>Gives the milliseconds left of the lockout.</summary>
        /// <param name="now">The current <see cref="Environment.TickCount64"/>.</param>
        /// <returns>The milliseconds left; 0 when the key is not locked out.</returns>
        public long LockedMs(long now)
        {
            lock (_lock)
                return Math.Max(0, _lockedUntilMs - now);
        }

        /// <summary>Whether the window and the lockout are over, so that the entry can be forgotten.</summary>
        /// <param name="now">The current <see cref="Environment.TickCount64"/>.</param>
        /// <param name="windowMs">The window in milliseconds.</param>
        /// <returns>True when nothing of the entry counts any more.</returns>
        public bool Expired(long now, long windowMs)
        {
            lock (_lock)
                return now - _windowStartMs >= windowMs && now >= _lockedUntilMs;
        }

        /// <summary>Starts a new window, with no events, once the current one is over.</summary>
        /// <param name="now">The current <see cref="Environment.TickCount64"/>.</param>
        /// <param name="windowMs">The window in milliseconds.</param>
        private void StartWindow(long now, long windowMs)
        {
            if (_count != 0 && now - _windowStartMs < windowMs)
                return;
            _windowStartMs = now;
            _count = 0;
        }
    }
}
