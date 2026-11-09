using System.Net;
using Microsoft.Extensions.Logging;

namespace NorthIsland.Server.Features.Admin;

/// <summary>The saved list of banned accounts and IP addresses.</summary>
public sealed class BanList
{
    /// <summary>The name of the document the list is saved as.</summary>
    private const string Document = "bans";

    /// <summary>The store the list is saved to.</summary>
    private readonly IDocumentStore _documents;

    /// <summary>The loaded bans.</summary>
    private readonly BanState _state;

    /// <summary>Logger for the save message.</summary>
    private readonly ILogger<BanList> _logger;

    /// <summary>Guards <c>_state</c>.</summary>
    private readonly Lock _lock = new();

    /// <summary>Loads the saved bans, or starts with none.</summary>
    /// <param name="documents">The document store holding the saved list.</param>
    /// <param name="logger">Logger for the save message.</param>
    public BanList(
        IDocumentStore documents,
        ILogger<BanList> logger)
    {
        _logger = logger;
        _documents = documents;
        _state = documents.Load<BanState>(Document) ?? new();
    }

    /// <summary>Tells whether an account is banned (account names compare without case).</summary>
    /// <param name="account">The account name.</param>
    /// <returns>True when the account is banned.</returns>
    public bool IsAccountBanned(string account)
    {
        lock (_lock)
            return _state.Accounts.Exists(b => string.Equals(b.Target, account, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>Tells whether an address is banned.</summary>
    /// <param name="address">The address; null is never banned.</param>
    /// <returns>True when the address is banned.</returns>
    public bool IsAddressBanned(IPAddress? address)
    {
        if (address is null)
            return false;
        var text = Normal(address);
        lock (_lock)
            return _state.Addresses.Exists(b => b.Target == text);
    }

    /// <summary>Bans an account and saves the list.</summary>
    /// <param name="account">The account name.</param>
    /// <param name="by">The name of the GM who bans.</param>
    /// <returns>False when the account was already banned.</returns>
    public bool BanAccount(string account, string by)
    {
        lock (_lock)
        {
            if (_state.Accounts.Exists(b => string.Equals(b.Target, account, StringComparison.OrdinalIgnoreCase)))
                return false;
            _state.Accounts.Add(new Ban { Target = account, Account = account, By = by, Utc = GameClock.UtcNow });
            Save();
            return true;
        }
    }

    /// <summary>Bans an address and saves the list.</summary>
    /// <param name="address">The address to ban.</param>
    /// <param name="account">The account the address was seen with.</param>
    /// <param name="by">The name of the GM who bans.</param>
    /// <returns>False when the address was already banned.</returns>
    public bool BanAddress(IPAddress address, string account, string by)
    {
        var text = Normal(address);
        lock (_lock)
        {
            if (_state.Addresses.Exists(b => b.Target == text))
                return false;
            _state.Addresses.Add(new Ban { Target = text, Account = account, By = by, Utc = GameClock.UtcNow });
            Save();
            return true;
        }
    }

    /// <summary>Lifts the bans of an account name, of an address, and of the addresses banned for that account.</summary>
    /// <param name="accountOrAddress">The account name or address text.</param>
    /// <returns>The number of bans lifted.</returns>
    public int Unban(string accountOrAddress)
    {
        lock (_lock)
        {
            var lifted = _state.Accounts.RemoveAll(b => string.Equals(b.Target, accountOrAddress, StringComparison.OrdinalIgnoreCase))
                + _state.Addresses.RemoveAll(
                    b => string.Equals(b.Account, accountOrAddress, StringComparison.OrdinalIgnoreCase) || b.Target == accountOrAddress);
            if (lifted > 0)
                Save();
            return lifted;
        }
    }

    /// <summary>The text of an address as stored; an IPv4-mapped IPv6 address becomes plain IPv4.</summary>
    /// <param name="address">The address.</param>
    /// <returns>The normalised address text.</returns>
    public static string Normal(IPAddress address) => (address.IsIPv4MappedToIPv6 ? address.MapToIPv4() : address).ToString();

    /// <summary>Saves the list to the document store and logs the counts.</summary>
    private void Save()
    {
        _documents.Save(Document, _state);
        _logger.LogInformation(
            "Saved {Accounts} account and {Addresses} address ban(s) to {Document}",
            _state.Accounts.Count,
            _state.Addresses.Count,
            Document);
    }
}
