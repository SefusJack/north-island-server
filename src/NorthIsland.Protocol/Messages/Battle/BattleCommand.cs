using NorthIsland.Protocol.IO;

namespace NorthIsland.Protocol.Messages.Battle;

/// <summary>A fighter's command read from the client: an attack or skill, or an item.</summary>
/// <param name="ActorRow">Battle row of the acting fighter.</param>
/// <param name="ActorCol">Battle column of the acting fighter.</param>
/// <param name="TargetRow">Battle row of the target.</param>
/// <param name="TargetCol">Battle column of the target.</param>
/// <param name="SkillOrItem">Skill id, or item id when <paramref name="IsItem"/> is set.</param>
/// <param name="Extra">Trailing byte of the packet, 0 when absent.</param>
/// <param name="IsItem">True when the command uses an item.</param>
public readonly record struct BattleCommand(
    byte ActorRow,
    byte ActorCol,
    byte TargetRow,
    byte TargetCol,
    ushort SkillOrItem,
    byte Extra,
    bool IsItem)
{
    /// <summary>Reads a command from a packet body: both cells, the id, two bytes that are skipped, and an optional trailing byte.</summary>
    /// <param name="body">The packet body after the action and sub code.</param>
    /// <param name="isItem">True when the packet is an item command.</param>
    /// <returns>The parsed command.</returns>
    public static BattleCommand Parse(ReadOnlySpan<byte> body, bool isItem)
    {
        var r = new PacketReader(body);
        var actorRow = r.U8();
        var actorCol = r.U8();
        var targetRow = r.U8();
        var targetCol = r.U8();
        var id = r.U16();
        r.U8();
        r.U8();
        return new BattleCommand(actorRow, actorCol, targetRow, targetCol, id, r.Remaining > 0 ? r.U8() : (byte)0, isItem);
    }
}
