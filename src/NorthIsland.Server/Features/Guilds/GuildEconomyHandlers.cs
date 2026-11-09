using Microsoft.Extensions.Logging;
using NorthIsland.Protocol.IO;
using NorthIsland.Protocol.Opcodes;
using NorthIsland.Protocol.Text;

namespace NorthIsland.Server.Features.Guilds;

/// <summary>Guild rights and guild mail: changing a member's rights and sending a letter to members.</summary>
/// <param name="guilds">The guild registry.</param>
/// <param name="world">The world state, to find members that are online.</param>
/// <param name="logger">The logger.</param>
public sealed class GuildEconomyHandlers(
    GuildRegistry guilds,
    WorldState world,
    ILogger<GuildEconomyHandlers> logger)
{
    /// <summary>The longest letter, in bytes of the game encoding.</summary>
    public const int MaxLetter = 200;

    /// <summary>The sessions of the guild's members that are online.</summary>
    /// <param name="guild">The guild.</param>
    /// <returns>The online sessions of its members.</returns>
    private IEnumerable<GameSession> Online(Guild guild)
    {
        List<int> ids;
        lock (guilds.Lock)
            ids = [.. guild.Members.Select(m => m.Id)];
        foreach (var id in ids)
        {
            if (world.SessionOf(id) is { } s)
                yield return s;
        }
    }

    /// <summary>Delivers the letters that waited for a member while they were offline and empties their mailbox.</summary>
    /// <param name="session">The member's session.</param>
    public void SendOnLogin(GameSession session)
    {
        var id = session.Account!.LoginId;
        if (guilds.Of(id) is not { } guild)
            return;
        lock (guilds.Lock)
        {
            if (guild.Mail.Remove(id, out var waiting))
            {
                foreach (var letter in waiting)
                    session.Send(MailPacket(letter));
                guilds.Save();
            }
        }
    }

    /// <summary>
    /// Handles <c>39/16</c>: the lord, or a member with the first right, switches rights of a member on or off and the online members are told.
    /// </summary>
    /// <param name="session">The sender's session.</param>
    /// <param name="body">The target's id, the number of changes, then a right number and an on/off value for each.</param>
    internal ValueTask OnRights(GameSession session, ReadOnlyMemory<byte> body)
    {
        if (InWorld(session) is null || body.Length < 5)
            return ValueTask.CompletedTask;
        var id = session.Account!.LoginId;
        var r = new PacketReader(body.Span);
        var (target, count) = (r.I32(), r.U8());
        if (guilds.Of(id) is not { } guild || count == 0 || body.Length < 5 + count * 2)
            return ValueTask.CompletedTask;
        var changes = new List<(byte Right, byte On)>();
        lock (guilds.Lock)
        {
            var me = guild.Member(id);
            var member = guild.Member(target);
            if (me is null || member is null || target == guild.LeaderId)
                return ValueTask.CompletedTask;
            var isLord = id == guild.LeaderId;
            if (!isLord && me.Rights[0] != 1)
                return ValueTask.CompletedTask;
            for (var i = 0; i < count; i++)
            {
                var (right, on) = (r.U8(), r.U8());
                if (right is < 1 or > 5 || on > 1 || (right == 1 && !isLord))
                    continue;
                member.Rights[right - 1] = on;
                changes.Add((right, on));
            }
            if (changes.Count == 0)
                return ValueTask.CompletedTask;
            guilds.Save();
        }
        var w = new PacketWriter(Action39.Code, Action39.MemberRights).I32(target).U8((byte)changes.Count);
        foreach (var (right, on) in changes)
            w.U8(right).U8(on);
        var packet = w.ToArray();
        foreach (var s in Online(guild))
            s.Send(packet);
        logger.LogInformation("[{Session}] guild rights of {Target} changed ({Count})", session.Id, target, changes.Count);
        return ValueTask.CompletedTask;
    }

    /// <summary>Builds the packet <c>39/5</c> that delivers a guild letter.</summary>
    /// <param name="m">The letter.</param>
    /// <returns>The packet.</returns>
    private static byte[] MailPacket(GuildMail m) =>
        new PacketWriter(Action39.Code, Action39.MailReceived).I32(m.From).F64(m.Time).Bytes(GameEncoding.GetBytes(m.Text)).ToArray();

    /// <summary>Handles <c>39/4</c>: sends a letter to guild members; online members get it at once, the others at their next login.</summary>
    /// <param name="session">The sender's session.</param>
    /// <param name="body">The number of recipients, their ids (four bytes each), then the letter text.</param>
    internal ValueTask OnMail(GameSession session, ReadOnlyMemory<byte> body)
    {
        if (InWorld(session) is not { } c || body.Length < 6)
            return ValueTask.CompletedTask;
        var id = session.Account!.LoginId;
        var n = body.Span[0];
        if (n == 0 || body.Length <= 1 + n * 4 || guilds.Of(id) is not { } guild)
            return ValueTask.CompletedTask;
        var text = GameEncoding.GetString(body.Span[(1 + n * 4)..]);
        if (text.Length == 0 || GameEncoding.GetBytes(text).Length > MaxLetter)
            return ValueTask.CompletedTask;
        var letter = new GuildMail { From = id, Time = GameClock.Now.ToOADate(), Text = text };
        var full = false;
        for (var i = 0; i < n; i++)
        {
            var to = System.Buffers.Binary.BinaryPrimitives.ReadInt32LittleEndian(body.Span[(1 + i * 4)..]);
            if (to == id || guild.Member(to) is null)
                continue;
            if (world.SessionOf(to) is { } target)
                target.Send(MailPacket(letter));
            else
            {
                lock (guilds.Lock)
                {
                    if (!guild.Mail.TryGetValue(to, out var box))
                        guild.Mail[to] = box = [];
                    if (box.Count >= GuildMail.Capacity)
                        full = true;
                    else
                        box.Add(letter);
                }
            }
        }
        if (!full || n > 0)
            guilds.Save();
        if (full)
            session.Send(new PacketWriter(Action39.Code, Action39.MailboxFull).ToArray());
        logger.LogInformation("[{Session}] {Name} sends guild mail to {Count} member(s)", session.Id, c.Name, n);
        return ValueTask.CompletedTask;
    }

    /// <summary>The active character of a session that is in the world.</summary>
    /// <param name="session">The session.</param>
    /// <returns>The character, or null when the session is not in the world.</returns>
    private static Character? InWorld(GameSession session)
    {
        var character = session.ActiveCharacter;
        return session.State == SessionState.InWorld && character is not null && session.Account is not null ? character : null;
    }
}
