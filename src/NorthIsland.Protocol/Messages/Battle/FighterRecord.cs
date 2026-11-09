using NorthIsland.Protocol.IO;

namespace NorthIsland.Protocol.Messages.Battle;

/// <summary>One fighter as sent in the 11/250 and 11/5 packets.</summary>
public sealed record FighterRecord
{
    /// <summary>
    /// State byte written before the kind; it is set for the receiving client's own fighter and 0 for the others. Left out in 11/5 records.
    /// </summary>
    public byte State { get; init; }

    /// <summary>Kind of fighter: 2 player, 4 pet, 7 monster.</summary>
    public required byte Kind { get; init; }

    /// <summary>Player id, or the Npc.dat id of a monster.</summary>
    public required int Id { get; init; }

    /// <summary>Role number sent with the fighter.</summary>
    public ushort Role { get; init; }

    /// <summary>Id of the player who owns this fighter, 0 when it has no owner.</summary>
    public int OwnerId { get; init; }

    /// <summary>Battle row of the fighter.</summary>
    public required byte Row { get; init; }

    /// <summary>Battle column of the fighter.</summary>
    public required byte Col { get; init; }

    /// <summary>Maximum HP.</summary>
    public int MaxHp { get; init; }

    /// <summary>Maximum SP.</summary>
    public ushort MaxSp { get; init; }

    /// <summary>Current HP.</summary>
    public int Hp { get; init; }

    /// <summary>Current SP.</summary>
    public ushort Sp { get; init; }

    /// <summary>Level.</summary>
    public byte Level { get; init; }

    /// <summary>Element byte.</summary>
    public byte Element { get; init; }

    /// <summary>Rebirth byte.</summary>
    public byte Reborn { get; init; }

    /// <summary>Job byte.</summary>
    public byte Job { get; init; }

    /// <summary>Appends the fighter record to a packet.</summary>
    /// <param name="w">The packet being built.</param>
    /// <param name="client">Client generation, which decides the width of the HP fields.</param>
    /// <param name="withState">True to write the leading state byte, false for 11/5 records.</param>
    public void WriteTo(PacketWriter w, ClientGeneration client, bool withState = true)
    {
        if (withState)
            w.U8(State);
        w.U8(Kind).I32(Id).U16(Role).I32(OwnerId).U8(Row).U8(Col)
            .Hp(MaxHp, client).U16(MaxSp).Hp(Hp, client).U16(Sp).U8(Level).U8(Element).U8(Reborn).U8(Job);
    }
}
