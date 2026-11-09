using NorthIsland.Protocol.IO;

namespace NorthIsland.Protocol.Messages.World;

/// <summary>Another player as the scene packets describe them; <see cref="WriteTo"/> writes the record.</summary>
public sealed record OtherPlayerRecord
{
    /// <summary>Player id.</summary>
    public required int Id { get; init; }

    /// <summary>Body type byte.</summary>
    public byte BodyType { get; init; }

    /// <summary>Element byte.</summary>
    public byte Element { get; init; }

    /// <summary>Level.</summary>
    public byte Level { get; init; }

    /// <summary>Id of the map the player is on.</summary>
    public required ushort MapId { get; init; }

    /// <summary>X cell.</summary>
    public ushort X { get; init; }

    /// <summary>Y cell.</summary>
    public ushort Y { get; init; }

    /// <summary>Byte written between the position and the head whose meaning is not known.</summary>
    public byte Unknown12 { get; init; }

    /// <summary>Head look byte.</summary>
    public byte Head { get; init; }

    /// <summary>Sprite byte.</summary>
    public byte Sprite { get; init; }

    /// <summary>First colour value.</summary>
    public int Colour1 { get; init; }

    /// <summary>Second colour value.</summary>
    public int Colour2 { get; init; }

    /// <summary>Item ids of the worn equipment, written with a count byte.</summary>
    public ushort[] EquippedItems { get; init; } = [];

    /// <summary>PvP win count.</summary>
    public ushort PvpWins { get; init; }

    /// <summary>PvP rank byte.</summary>
    public byte PvpRank { get; init; }

    /// <summary>Online status byte.</summary>
    public byte OnlineStatus { get; init; }

    /// <summary>Arena victory byte.</summary>
    public byte ArenaVictory { get; init; }

    /// <summary>Reborn count.</summary>
    public byte Reborn { get; init; }

    /// <summary>Job byte.</summary>
    public byte Job { get; init; }

    /// <summary>Player name.</summary>
    public required string Name { get; init; }

    /// <summary>Nickname; empty for none.</summary>
    public string Nickname { get; init; } = "";

    /// <summary>Heart number, written as the last byte of the record.</summary>
    public byte HeartNumber { get; init; }

    /// <summary>Writes the record in packet order.</summary>
    /// <param name="w">The writer to append to.</param>
    public void WriteTo(PacketWriter w)
    {
        w.I32(Id).U8(BodyType).U8(Element).U8(Level).U16(MapId).U16(X).U16(Y)
            .U8(Unknown12).U8(Head).U8(Sprite)
            .I32(Colour1).I32(Colour2)
            .U8((byte)EquippedItems.Length);
        foreach (var item in EquippedItems)
            w.U16(item);
        w.U16(PvpWins).U8(PvpRank).U8(OnlineStatus).U8(ArenaVictory).U8(Reborn).U8(Job)
            .PStr8(Name).PStr8(Nickname)
            .U8(HeartNumber);
    }
}
