using NorthIsland.Protocol.IO;

namespace NorthIsland.Protocol.Messages.Player;

/// <summary>The player's attribute record that the HUD and the status window show.</summary>
public sealed record PlayerAttributes
{
    /// <summary>The player's element.</summary>
    public byte Element { get; init; }

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

    /// <summary>Level.</summary>
    public byte Level { get; init; }

    /// <summary>Experience points.</summary>
    public int Exp { get; init; }

    /// <summary>Skill points to spend.</summary>
    public ushort SkillPoints { get; init; }

    /// <summary>Stat points to spend.</summary>
    public ushort StatPoints { get; init; }

    /// <summary>Maximum HP.</summary>
    public int MaxHp { get; init; }

    /// <summary>Maximum SP.</summary>
    public ushort MaxSp { get; init; }

    /// <summary>Attack bonus.</summary>
    public int AtkBonus { get; init; }

    /// <summary>Defence bonus.</summary>
    public int DefBonus { get; init; }

    /// <summary>Magic attack bonus.</summary>
    public int MatkBonus { get; init; }

    /// <summary>Speed bonus.</summary>
    public int SpdBonus { get; init; }

    /// <summary>Maximum HP bonus.</summary>
    public int MaxHpBonus { get; init; }

    /// <summary>Maximum SP bonus.</summary>
    public int MaxSpBonus { get; init; }

    /// <summary>Id of the player's guild.</summary>
    public uint GuildId { get; init; }

    /// <summary>The player's skills.</summary>
    public IReadOnlyList<SkillEntry> Skills { get; init; } = [];

    /// <summary>Extra maximum HP.</summary>
    public ushort MaxHpExtra { get; init; }

    /// <summary>Extra maximum SP.</summary>
    public ushort MaxSpExtra { get; init; }

    /// <summary>How many times the player was reborn.</summary>
    public byte Reborn { get; init; }

    /// <summary>The player's job.</summary>
    public byte Job { get; init; }

    /// <summary>Potential value.</summary>
    public byte Potential { get; init; }

    /// <summary>Writes the record.</summary>
    /// <param name="w">The packet writer.</param>
    /// <param name="client">The client generation the record is written for.</param>
    public void WriteTo(PacketWriter w, ClientGeneration client)
    {
        w.U8(Element).Hp(Hp, client).U16(Sp)
            .U16(Strength).U16(Constitution).U16(Intelligence).U16(Wisdom).U16(Agility)
            .U8(Level).I32(Exp)
            .U16(SkillPoints).U16(StatPoints).Hp(MaxHp, client).U16(MaxSp)
            .I32(AtkBonus).I32(DefBonus).I32(MatkBonus).I32(SpdBonus).I32(MaxHpBonus).I32(MaxSpBonus)
            .U32(GuildId)
            .U16((ushort)Skills.Count);
        foreach (var skill in Skills)
        {
            if (skill.Data.Length != SkillEntry.DataSize)
                throw new ArgumentException($"Skill {skill.Key} has {skill.Data.Length} data bytes, expected {SkillEntry.DataSize}.");
            w.U16(skill.Key).Bytes(skill.Data);
        }
        w.U16(MaxHpExtra).U16(MaxSpExtra).U8(Reborn).U8(Job).U8(Potential);
    }
}
