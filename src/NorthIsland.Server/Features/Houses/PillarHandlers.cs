using Microsoft.Extensions.Logging;
using NorthIsland.Protocol.IO;
using NorthIsland.Protocol.Opcodes;

namespace NorthIsland.Server.Features.Houses;

/// <summary>
/// Handles the pillars of a house: the owner's changes from the Handcart's pillar form, and sending the stored pillars to everyone who enters
/// the house.
/// </summary>
/// <param name="world">The world state, used to find the sessions in a house.</param>
/// <param name="tents">The tent handlers, whose house-entered event sends the pillars.</param>
/// <param name="logger">The logger.</param>
public sealed class PillarHandlers(
    WorldState world,
    TentHandlers tents,
    ILogger<PillarHandlers> logger)
{
    /// <summary>The item id of the Handcart, the furniture piece that opens the pillar form.</summary>
    public const ushort Handcart = 39065; // Handcart

    /// <summary>
    /// The size of the pillar data in bytes (<c>Bytes</c>, both floors together) and the number of bytes that belong to the ground floor,
    /// which is the offset of the upper floor's wire indexes (<c>UpperOffset</c>).
    /// </summary>
    public const int Bytes = 48, UpperOffset = 22;

    /// <summary>Hooks the handler into the tent house-entered event.</summary>
    internal void Start()
    {
        tents.HouseEntered += OnHouseEntered;
    }

    /// <summary>Returns the pillar bytes of a floor.</summary>
    /// <param name="c">The house owner's character.</param>
    /// <param name="floor">The floor: 0 for the ground floor, 1 for the upper floor.</param>
    /// <returns>The live pillar array of that floor, not a copy.</returns>
    private static byte[] Bits(Character c, int floor) => floor == 0 ? c.PillarsGround : c.PillarsUpper;

    /// <summary>Returns the amount subtracted from a byte index to get the index on the wire for a floor.</summary>
    /// <param name="floor">The floor: 0 for the ground floor, 1 for the upper floor.</param>
    /// <returns>0 for the ground floor, <c>UpperOffset</c> for the upper floor.</returns>
    private static int Offset(int floor) => floor == 0 ? 0 : UpperOffset;

    /// <summary>
    /// Handles the pillar changes of the house owner (<c>65/11</c>): all bytes or index and bits pairs are applied to the floor the owner stands on
    /// when a Handcart is placed there, and the changed bytes are sent to the others in the house.
    /// </summary>
    /// <param name="session">The player's session.</param>
    /// <param name="body">The packet body after the action and sub code.</param>
    internal ValueTask OnChanges(GameSession session, ReadOnlyMemory<byte> body)
    {
        var c = session.ActiveCharacter;
        if (session.State != SessionState.InWorld || c is null || session.Account is null || session.HouseRoom > 1 || body.Length < 3
            || TentHandlers.HouseMapOf(session.Account.LoginId) != c.MapId)
            return ValueTask.CompletedTask;
        var floor = session.HouseRoom;
        var furn = (ushort)(body.Span[0] | body.Span[1] << 8);
        if (!c.Furniture.Exists(f => f.Index == furn && f.ItemId == Handcart && f.Room == floor && !f.Retained))
            return ValueTask.CompletedTask;
        var data = body.Span[3..];
        var changes = new List<(int Index, byte Bits)>();
        if (body.Span[2] == 1)
        {
            for (var i = 1; i <= Math.Min(Bytes, data.Length); i++)
                changes.Add((i, data[i - 1]));
        }
        else if (body.Span[2] == 2)
        {
            for (var i = 0; i + 1 < data.Length; i += 2)
                changes.Add((data[i] + Offset(floor), data[i + 1]));
        }
        else
            return ValueTask.CompletedTask;
        var bits = Bits(c, floor);
        var low = floor == 0 ? 1 : UpperOffset + 1;
        var applied = new List<(int Index, byte Bits)>();
        foreach (var (index, value) in changes)
        {
            if (index < low || index > Bytes || bits[index - 1] == value)
                continue;
            bits[index - 1] = value;
            applied.Add((index, value));
        }
        if (applied.Count == 0)
            return ValueTask.CompletedTask;
        var w = new PacketWriter(Action65.Code, Action65.PillarDiff).U8((byte)floor).U8(2);
        foreach (var (index, value) in applied)
            w.U8((byte)(index - Offset(floor))).U8(value);
        var packet = w.ToArray();
        foreach (var other in world.OnMap(c.MapId).Where(o => o != session))
            other.Send(packet);
        logger.LogInformation("[{Session}] {Name} changes {Count} pillar byte(s) on floor {Floor}", session.Id, c.Name, applied.Count, floor);
        return ValueTask.CompletedTask;
    }

    /// <summary>Sends the entering player the stored pillars of both floors of the house, when there are any.</summary>
    /// <param name="session">The session of the player who entered.</param>
    /// <param name="owner">The session of the house's owner; null when the owner is not online.</param>
    private void OnHouseEntered(GameSession session, GameSession? owner)
    {
        var host = owner?.ActiveCharacter ?? tents.OwnerOfHouse(session.ActiveCharacter?.MapId ?? 0)?.Character;
        if (host is null)
            return;
        var w = new PacketWriter(Action65.Code, Action65.Pillars);
        var any = false;
        for (var floor = 0; floor <= 1; floor++)
        {
            var bits = Bits(host, floor);
            var set = Enumerable.Range(1, Bytes).Where(i => bits[i - 1] != 0 && i > Offset(floor)).ToList();
            if (set.Count == 0)
                continue;
            any = true;
            w.U8((byte)floor).U8(2).U8((byte)set.Count);
            foreach (var i in set)
                w.U8((byte)(i - Offset(floor))).U8(bits[i - 1]);
        }
        if (any)
            session.Send(w.ToArray());
    }
}
