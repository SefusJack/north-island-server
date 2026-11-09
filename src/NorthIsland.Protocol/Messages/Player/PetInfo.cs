namespace NorthIsland.Protocol.Messages.Player;

/// <summary>A pet as the pet packets describe it.</summary>
public sealed record PetInfo
{
    /// <summary>The pet's slot in the team.</summary>
    public byte Slot { get; init; }

    /// <summary>The pet's npc id.</summary>
    public required ushort NpcId { get; init; }

    /// <summary>The pet's name.</summary>
    public string Name { get; init; } = "";

    /// <summary>Experience points.</summary>
    public int Exp { get; init; }

    /// <summary>Level.</summary>
    public byte Level { get; init; }

    /// <summary>Current HP.</summary>
    public int Hp { get; init; }

    /// <summary>Current SP.</summary>
    public ushort Sp { get; init; }

    /// <summary>Strength.</summary>
    public ushort Strength { get; init; }

    /// <summary>Constitution.</summary>
    public ushort Constitution { get; init; }

    /// <summary>Intelligence.</summary>
    public ushort Intelligence { get; init; }

    /// <summary>Wisdom.</summary>
    public ushort Wisdom { get; init; }

    /// <summary>Agility.</summary>
    public ushort Agility { get; init; }

    /// <summary>Amity.</summary>
    public byte Amity { get; init; }

    /// <summary>Tradable flag byte.</summary>
    public byte Tradable { get; init; }

    /// <summary>Skill points to spend.</summary>
    public ushort SkillPoints { get; init; }

    /// <summary>How many times the pet was reborn.</summary>
    public byte Reborn { get; init; }

    /// <summary>Potential value.</summary>
    public byte Potential { get; init; }

    /// <summary>The pet's skills, level and experience of each; three are always written.</summary>
    public IReadOnlyList<(byte Level, int Exp)> Skills { get; init; } = [];

    /// <summary>Items the pet wears, at most six.</summary>
    public IReadOnlyList<Items.EquipEntry> Equipment { get; init; } = [];

    /// <summary>The skills padded with empty ones to exactly three.</summary>
    /// <returns>Level and experience of the three skill slots.</returns>
    internal IEnumerable<(byte Level, int Exp)> SkillSlots() => Skills.Concat(Enumerable.Repeat(((byte)0, 0), 3)).Take(3);
}
