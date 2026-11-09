using NorthIsland.Protocol.IO;
using NorthIsland.Protocol.Opcodes;

namespace NorthIsland.Protocol.Messages.Player;

/// <summary>Builders for the guild packets of action 39, all server to client.</summary>
public static class GuildMessages
{
    /// <summary>Refuses a guild war request.</summary>
    /// <param name="variant">The refusal variant byte.</param>
    /// <returns>The packet.</returns>
    public static byte[] WarRefused(byte variant) => new PacketWriter(Action39.Code, Action39.WarRefused).U8(variant).ToArray();

    /// <summary>Sends the online state of guild members.</summary>
    /// <param name="members">Id and online flag of each member.</param>
    /// <returns>The packet.</returns>
    public static byte[] MemberStates(IEnumerable<(int Id, bool Online)> members)
    {
        var w = new PacketWriter(Action39.Code, Action39.MemberStates);
        foreach (var (id, online) in members)
            w.I32(id).U8(online ? (byte)0 : (byte)3);
        return w.ToArray();
    }

    /// <summary>Number of vice slots in the guild info record.</summary>
    public const int ViceSlots = 4;

    /// <summary>Size in bytes of a guild badge.</summary>
    public const int BadgeSize = 288;

    /// <summary>Lists the guilds that have a badge with their versions (<c>39/31</c>).</summary>
    /// <param name="badges">Guild id and badge version of each badge.</param>
    /// <returns>The packet.</returns>
    public static byte[] BadgeList(IEnumerable<(uint GuildId, byte Version)> badges)
    {
        var w = new PacketWriter(Action39.Code, Action39.BadgeList);
        foreach (var (id, version) in badges)
            w.U32(id).U8(version);
        return w.ToArray();
    }

    /// <summary>Sends badges.</summary>
    /// <param name="badges">Guild id, version and badge data of each badge.</param>
    /// <returns>The packet.</returns>
    public static byte[] Badges(IEnumerable<(uint GuildId, byte Version, byte[] Data)> badges)
    {
        var w = new PacketWriter(Action39.Code, Action39.Badges);
        foreach (var (id, version, data) in badges)
            w.U32(id).U8(version).Bytes(data);
        return w.ToArray();
    }

    /// <summary>Sends the player's own guild badge.</summary>
    /// <param name="version">The badge version.</param>
    /// <param name="data">The badge data.</param>
    /// <returns>The packet.</returns>
    public static byte[] OwnBadge(byte version, byte[] data) =>
        new PacketWriter(Action39.Code, Action39.OwnBadge).U8(version).Bytes(data).ToArray();

    /// <summary>Sends the whole guild (<c>39/2</c>): name, lord, vices, members, rules and the founding date.</summary>
    /// <param name="name">The guild's name.</param>
    /// <param name="lord">The guild lord.</param>
    /// <param name="vices">The vice lords.</param>
    /// <param name="members">The other members.</param>
    /// <param name="rules">The guild rules text.</param>
    /// <param name="founded">Founding date as an OLE automation date.</param>
    /// <param name="allyClosed">True when the ally function is closed.</param>
    /// <returns>The packet.</returns>
    public static byte[] Info(
        string name,
        GuildMemberInfo lord,
        IReadOnlyList<GuildMemberInfo> vices,
        IReadOnlyList<GuildMemberInfo> members,
        string rules,
        double founded,
        bool allyClosed = false)
    {
        var w = new PacketWriter(Action39.Code, Action39.Info).PStr8(name).U8((byte)vices.Count).U8((byte)(1 + ViceSlots + members.Count));
        Member(w, lord);
        foreach (var v in vices.Take(ViceSlots))
            Member(w, v);
        foreach (var m in members)
            Member(w, m);
        return w.PStr8(rules).F64(0).F64(founded).U8(allyClosed ? (byte)1 : (byte)0).ToArray();
    }

    /// <summary>Writes one member record.</summary>
    /// <param name="w">The packet writer.</param>
    /// <param name="m">The member.</param>
    private static void Member(PacketWriter w, GuildMemberInfo m)
    {
        w.I32(m.Id).PStr8(m.Name).U8(m.Level).U8(m.Job).U8(m.Reborn).U8(m.Element).U8(m.BodyType).U8(m.Head)
            .I32(m.Colour1).I32(m.Colour2).PStr8(m.Nickname).I32(m.Contribution);
        for (var i = 0; i < 5; i++)
            w.U8(i < m.Rights.Length ? m.Rights[i] : (byte)0);
        w.Bytes(new byte[5]);
    }

    /// <summary>Answers a guild creation.</summary>
    /// <param name="result">The result code.</param>
    /// <returns>The packet.</returns>
    public static byte[] CreateResult(byte result) => new PacketWriter(Action39.Code, Action39.Create).U8(result).ToArray();

    /// <summary>Invites a player to a guild.</summary>
    /// <param name="inviterId">Id of the inviting player.</param>
    /// <returns>The packet.</returns>
    public static byte[] Invitation(int inviterId) => new PacketWriter(Action39.Code, Action39.Answer).I32(inviterId).ToArray();

    /// <summary>Tells the inviter how the invitation went.</summary>
    /// <param name="targetId">Id of the invited player.</param>
    /// <param name="result">The result code.</param>
    /// <returns>The packet.</returns>
    public static byte[] InviteResult(int targetId, byte result) =>
        new PacketWriter(Action39.Code, Action39.InviteResult).I32(targetId).U8(result).ToArray();

    /// <summary>Tells the guild that a member left.</summary>
    /// <param name="id">Id of the member.</param>
    /// <returns>The packet.</returns>
    public static byte[] MemberRemoved(int id) => new PacketWriter(Action39.Code, Action39.Leave).I32(id).ToArray();

    /// <summary>Answers a leave or kick.</summary>
    /// <param name="result">The result code.</param>
    /// <param name="id">Id of the member concerned.</param>
    /// <returns>The packet.</returns>
    public static byte[] LeaveResult(byte result, int id) => new PacketWriter(Action39.Code, Action39.Kick).U8(result).I32(id).ToArray();

    /// <summary>Tells the guild that a member joined.</summary>
    /// <param name="id">Id of the member.</param>
    /// <returns>The packet.</returns>
    public static byte[] MemberJoined(int id) => new PacketWriter(Action39.Code, Action39.Joined).I32(id).ToArray();

    /// <summary>Sends the guild tags shown above players.</summary>
    /// <param name="tags">Player id, guild id and guild name of each tag.</param>
    /// <returns>The packet.</returns>
    public static byte[] Tags(IEnumerable<(int PlayerId, uint GuildId, string GuildName)> tags)
    {
        var w = new PacketWriter(Action39.Code, Action39.Tags);
        foreach (var (id, guildId, name) in tags)
            w.I32(id).U32(guildId).PStr8(name);
        return w.ToArray();
    }

    /// <summary>Tells that the guild was disbanded.</summary>
    /// <param name="result">The result code.</param>
    /// <returns>The packet.</returns>
    public static byte[] Disbanded(byte result) => new PacketWriter(Action39.Code, Action39.Disbanded).U8(result).ToArray();

    /// <summary>Sends the changed guild rules text.</summary>
    /// <param name="rules">The new rules.</param>
    /// <returns>The packet.</returns>
    public static byte[] RulesChanged(string rules) => new PacketWriter(Action39.Code, Action39.RulesChanged).RawString(rules).ToArray();

    /// <summary>Tells that a vice lord was appointed or deposed.</summary>
    /// <param name="id">Id of the member.</param>
    /// <param name="appointed">True when appointed, false when deposed.</param>
    /// <returns>The packet.</returns>
    public static byte[] ViceChanged(int id, bool appointed) =>
        new PacketWriter(Action39.Code, appointed ? Action39.ViceAppointed : Action39.ViceDeposed).I32(id).ToArray();

    /// <summary>Tells the guild its new lord.</summary>
    /// <param name="newLordId">Id of the new lord.</param>
    /// <returns>The packet.</returns>
    public static byte[] NewLord(int newLordId) => new PacketWriter(Action39.Code, Action39.NewLord).U8(3).I32(newLordId).ToArray();

    /// <summary>Reports a failed appointment.</summary>
    /// <param name="code">The error code.</param>
    /// <returns>The packet.</returns>
    public static byte[] PositionError(byte code) => new PacketWriter(Action39.Code, Action39.PositionError).U8(code).ToArray();

    /// <summary>Offers a guild post to a player.</summary>
    /// <param name="fromId">Id of the offering player.</param>
    /// <param name="lord">True for the lord post, false for a vice post.</param>
    /// <returns>The packet.</returns>
    public static byte[] Offer(int fromId, bool lord) =>
        new PacketWriter(Action39.Code, lord ? Action39.LordOffer : Action39.ViceOffer).I32(fromId).ToArray();

    /// <summary>Tells the offerer that the post was accepted.</summary>
    /// <param name="id">Id of the accepting player.</param>
    /// <param name="lord">True for the lord post, false for a vice post.</param>
    /// <returns>The packet.</returns>
    public static byte[] PostAccepted(int id, bool lord) =>
        new PacketWriter(Action39.Code, Action39.PostAccepted).U8(lord ? (byte)2 : (byte)1).I32(id).ToArray();

    /// <summary>Opens the guild creation form.</summary>
    /// <returns>The packet.</returns>
    public static byte[] OpenCreate() => new PacketWriter(Action39.Code, Action39.OpenCreate).ToArray();

    /// <summary>Greets a new guild member.</summary>
    /// <returns>The packet.</returns>
    public static byte[] Welcome() => new PacketWriter(Action39.Code, Action39.Welcome).U8(1).ToArray();
}
