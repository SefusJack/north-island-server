using NorthIsland.Protocol.IO;

namespace NorthIsland.Protocol.Messages.Login;

/// <summary>One character in the character list the login screen shows.</summary>
public sealed record CharacterSummary
{
    /// <summary>The character's slot in the account.</summary>
    public required byte Slot { get; init; }

    /// <summary>The character's name.</summary>
    public required string Name { get; init; }

    /// <summary>The character's level.</summary>
    public byte Level { get; init; }

    /// <summary>The character's element.</summary>
    public byte Element { get; init; }

    /// <summary>Maximum HP.</summary>
    public int MaxHp { get; init; }

    /// <summary>Current HP.</summary>
    public int Hp { get; init; }

    /// <summary>Maximum SP.</summary>
    public int MaxSp { get; init; }

    /// <summary>Current SP.</summary>
    public int Sp { get; init; }

    /// <summary>Experience points.</summary>
    public int Exp { get; init; }

    /// <summary>Gold carried.</summary>
    public int Gold { get; init; }

    /// <summary>Body type of the character's model.</summary>
    public byte BodyType { get; init; }

    /// <summary>A byte of unknown meaning, written between the body type and the head.</summary>
    public byte Unknown33 { get; init; }

    /// <summary>Head of the character's model.</summary>
    public byte Head { get; init; }

    /// <summary>Sprite of the character's model.</summary>
    public byte Sprite { get; init; }

    /// <summary>First colour value of the character's model.</summary>
    public int Colour1 { get; init; }

    /// <summary>Second colour value of the character's model.</summary>
    public int Colour2 { get; init; }

    /// <summary>How many times the character was reborn.</summary>
    public byte RebornCount { get; init; }

    /// <summary>The character's job.</summary>
    public byte Job { get; init; }

    /// <summary>Item ids shown on the character, six are written (missing ones as 0).</summary>
    public ushort[] EquippedItems { get; init; } = [];

    /// <summary>Writes the record.</summary>
    /// <param name="w">The packet writer.</param>
    public void WriteTo(PacketWriter w)
    {
        w.U8(Slot).PStr8(Name)
            .U8(Level).U8(Element)
            .I32(MaxHp).I32(Hp).I32(MaxSp).I32(Sp).I32(Exp).I32(Gold)
            .U8(BodyType).U8(Unknown33).U8(Head).U8(Sprite)
            .I32(Colour1).I32(Colour2)
            .U8(RebornCount).U8(Job);
        for (var i = 0; i < 6; i++)
            w.U16(i < EquippedItems.Length ? EquippedItems[i] : (ushort)0);
    }
}
