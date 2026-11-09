using Microsoft.Extensions.Logging;
using NorthIsland.Protocol.IO;
using NorthIsland.Protocol.Opcodes;

namespace NorthIsland.Server.Features.Houses;

/// <summary>Decorative pieces in a house: the New Year Cracker, the Gugu Clock and the picture frames.</summary>
/// <param name="world">The world state, to find the sessions in a house.</param>
/// <param name="tents">The house and furniture handlers, for house owners.</param>
/// <param name="marriages">The marriage registry, to show the owner's spouse in a picture.</param>
/// <param name="logger">The logger.</param>
public sealed class HouseFunHandlers(
    WorldState world,
    TentHandlers tents,
    MarriageRegistry marriages,
    ILogger<HouseFunHandlers> logger)
{
    /// <summary>The item ids of the New Year Cracker and of the Gugu Clock.</summary>
    public const ushort Cracker = 39149, Clock = 39152; // 39149 New Year Cracker, 39152 Gugu Clock

    /// <summary>Handles <c>62/66</c>: answers with <c>62/62</c> the people a picture frame shows, the owner alone or with their spouse.</summary>
    /// <param name="session">The session of the player who clicked the frame.</param>
    /// <param name="body">The furniture index.</param>
    internal ValueTask OnPortrait(GameSession session, ReadOnlyMemory<byte> body)
    {
        var c = session.ActiveCharacter;
        if (session.State != SessionState.InWorld || c is null || body.Length < 1 || !TentHandlers.IsHouseMap(c.MapId))
            return ValueTask.CompletedTask;
        var owner = tents.OwnerOfHouse(c.MapId);
        if (owner is null || !owner.Character.Furniture.Exists(f => f.Index == body.Span[0]
            && f.ItemId is 39153 // Picture Frame
                or 39154 // Good Picture Frame
            && !f.Retained))
            return ValueTask.CompletedTask;
        var w = new PacketWriter(Action62.Code, Action62.PortraitReply);
        if (marriages.CoupleOf(owner.Id) is { } couple)
        {
            var spouse = couple.SpouseOf(owner.Id);
            w.U8(1).U8(spouse.BodyType).U8(spouse.Head).I32(spouse.Colour1).I32(spouse.Colour2);
        }
        else
            w.U8(0);
        session.Send(w.ToArray());
        return ValueTask.CompletedTask;
    }

    /// <summary>Handles a click on the New Year Cracker (<c>62/64</c>) or the Gugu Clock (<c>62/65</c>) and lets the house see or hear it.</summary>
    /// <param name="session">The session of the player who clicked.</param>
    /// <param name="body">The furniture index (two bytes).</param>
    /// <param name="kind">The item id of the clicked piece, <see cref="Cracker"/> or <see cref="Clock"/>.</param>
    internal ValueTask Click(GameSession session, ReadOnlyMemory<byte> body, ushort kind)
    {
        var c = session.ActiveCharacter;
        if (session.State != SessionState.InWorld || c is null || session.Battle is not null || body.Length < 2 || !TentHandlers.IsHouseMap(c.MapId))
            return ValueTask.CompletedTask;
        var host = tents.OwnerOfHouse(c.MapId)?.Character;
        var index = (ushort)(body.Span[0] | body.Span[1] << 8);
        if (host?.Furniture.Find(f => f.Index == index && f.ItemId == kind && !f.Retained && f.Room == session.HouseRoom) is not { } piece || index > 255)
            return ValueTask.CompletedTask;
        var house = world.OnMap(c.MapId).Append(session).Distinct().ToList();
        if (kind == Cracker)
        {
            var packet = new PacketWriter(Action62.Code, Action62.CrackerPlay).U8(piece.Room).U8((byte)index).ToArray();
            house.ForEach(s => s.Send(packet));
        }
        else
        {
            var packet = new PacketWriter(Action62.Code, Action62.ClockChime).U8(1).U8(piece.Room).U8((byte)index).ToArray();
            house.ForEach(s => s.Send(packet));
            session.Send(new PacketWriter(Action62.Code, Action62.ClockChime).U8(2).F64(GameClock.Now.ToOADate()).ToArray());
        }
        logger.LogInformation(
            "[{Session}] {Name} clicks {Piece} {Index}",
            session.Id,
            c.Name,
            kind == Cracker ? "the New Year Cracker" : "the Gugu Clock",
            index);
        return ValueTask.CompletedTask;
    }
}
