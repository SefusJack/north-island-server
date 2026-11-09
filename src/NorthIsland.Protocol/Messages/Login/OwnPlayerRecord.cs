using NorthIsland.Protocol.IO;

namespace NorthIsland.Protocol.Messages.Login;

/// <summary>The player's own record that enters the world.</summary>
public sealed record OwnPlayerRecord
{
    /// <summary>The player's id.</summary>
    public required int Id { get; init; }

    /// <summary>Body type of the player's model.</summary>
    public byte BodyType { get; init; }

    /// <summary>Id of the map the player is on.</summary>
    public required ushort MapId { get; init; }

    /// <summary>X position on the map.</summary>
    public ushort X { get; init; }

    /// <summary>Y position on the map.</summary>
    public ushort Y { get; init; }

    /// <summary>A byte of unknown meaning, written after the position.</summary>
    public byte Unknown12 { get; init; }

    /// <summary>Head of the player's model.</summary>
    public byte Head { get; init; }

    /// <summary>Sprite of the player's model.</summary>
    public byte Sprite { get; init; }

    /// <summary>First colour value of the player's model.</summary>
    public int Colour1 { get; init; }

    /// <summary>Second colour value of the player's model.</summary>
    public int Colour2 { get; init; }

    /// <summary>Item ids worn; the count is written first.</summary>
    public ushort[] EquippedItems { get; init; } = [];

    /// <summary>PvP wins.</summary>
    public ushort PvpWins { get; init; }

    /// <summary>PvP rank.</summary>
    public byte PvpRank { get; init; }

    /// <summary>Arena victory byte.</summary>
    public byte ArenaVictory { get; init; }

    /// <summary>The player's name.</summary>
    public required string Name { get; init; }

    /// <summary>The player's nickname.</summary>
    public string Nickname { get; init; } = "";

    /// <summary>Blood type.</summary>
    public byte BloodType { get; init; }

    /// <summary>Year of birth.</summary>
    public byte BirthYear { get; init; }

    /// <summary>Month of birth.</summary>
    public byte BirthMonth { get; init; }

    /// <summary>Day of birth.</summary>
    public byte BirthDay { get; init; }

    /// <summary>Writes the record.</summary>
    /// <param name="w">The packet writer.</param>
    public void WriteTo(PacketWriter w)
    {
        w.I32(Id).U8(BodyType).U16(MapId).U16(X).U16(Y)
            .U8(Unknown12).U8(Head).U8(Sprite)
            .I32(Colour1).I32(Colour2)
            .U8((byte)EquippedItems.Length);
        foreach (var item in EquippedItems)
            w.U16(item);
        w.U16(PvpWins).U8(PvpRank).U8(ArenaVictory)
            .PStr8(Name).PStr8(Nickname)
            .U8(BloodType).U8(BirthYear).U8(BirthMonth).U8(BirthDay);
    }
}
