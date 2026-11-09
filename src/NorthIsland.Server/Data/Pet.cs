namespace NorthIsland.Server.Data;

/// <summary>A pet owned by a character, or one that departed from the team.</summary>
public sealed class Pet
{
    /// <summary>Most pets a character carries.</summary>
    public const int MaxPets = 4;

    /// <summary>True once the pet has received its starting equipment, so that it is given only once.</summary>
    public bool StartingGearGiven { get; set; }

    /// <summary>When a hired pet goes back to its owner, or null for a pet that is the character's own.</summary>
    public DateTime? LeaseUntil { get; set; }

    /// <summary>Amity a new pet starts with.</summary>
    public const byte StartingAmity = 60;

    /// <summary>Progress of the pet's house training, in percent.</summary>
    public byte TrainPercent { get; set; }

    /// <summary>Level the pet had when the current training began; training progress is dropped when the level differs.</summary>
    public byte TrainLevel { get; set; }

    /// <summary>Pet slot, 1 to <see cref="MaxPets"/>.</summary>
    public byte Slot { get; set; }

    /// <summary>Npc id of the pet's kind.</summary>
    public ushort NpcId { get; set; }

    /// <summary>The pet's name.</summary>
    public string Name { get; set; } = "";

    /// <summary>Current level.</summary>
    public byte Level { get; set; } = 1;

    /// <summary>Absolute experience.</summary>
    public int Exp { get; set; }

    /// <summary>Current HP.</summary>
    public int Hp { get; set; }

    /// <summary>Current SP.</summary>
    public int Sp { get; set; }

    /// <summary>Maximum HP.</summary>
    public int MaxHp { get; set; }

    /// <summary>Maximum SP.</summary>
    public int MaxSp { get; set; }

    /// <summary>Strength stat.</summary>
    public ushort Strength { get; set; }

    /// <summary>Constitution stat.</summary>
    public ushort Constitution { get; set; }

    /// <summary>Intelligence stat.</summary>
    public ushort Intelligence { get; set; }

    /// <summary>Wisdom stat.</summary>
    public ushort Wisdom { get; set; }

    /// <summary>Agility stat.</summary>
    public ushort Agility { get; set; }

    /// <summary>The pet's element.</summary>
    public byte Element { get; set; }

    /// <summary>Amity towards the owner.</summary>
    public byte Amity { get; set; } = StartingAmity;

    /// <summary>1 while the pet may be renamed and traded; sent to the client as given.</summary>
    public byte Tradable { get; set; } = 1;

    /// <summary>Skill points the pet has to spend.</summary>
    public ushort SkillPoints { get; set; }

    /// <summary>Number of times the pet has been reborn.</summary>
    public byte Reborn { get; set; }

    /// <summary>Potential value of the pet, as set by the potential pill.</summary>
    public byte Potential { get; set; }

    /// <summary>Levels of the pet's three skill slots; 0 means the slot has no skill.</summary>
    public byte[] SkillLevels { get; set; } = [0, 0, 0];

    /// <summary>Experience of the pet's three skill slots.</summary>
    public int[] SkillExp { get; set; } = [0, 0, 0];

    /// <summary>The items the pet wears.</summary>
    public List<EquippedItem> Equipment { get; set; } = [];
}
