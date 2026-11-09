using NorthIsland.GameData.Npcs;
using NorthIsland.Protocol.IO;
using NorthIsland.Protocol.Opcodes;

namespace NorthIsland.Server.Features.Battles;

/// <summary>Builds and sends the monster book packets and records the monsters a player has defeated.</summary>
public static class MonsterBookHandlers
{
    /// <summary>Builds the server packet that adds one monster to the book.</summary>
    /// <param name="npcId">The Npc.dat id of the monster.</param>
    /// <returns>The packet <c>53/9</c>.</returns>
    public static byte[] Added(int npcId) => new PacketWriter(Action53.Code, Action53.BookAdded).I32(npcId).ToArray();

    /// <summary>Builds the server packet that sets many book keys at once, as groups of 8 bits.</summary>
    /// <param name="keys">The Npc.dat keys to set; keys outside 1..2040 are skipped.</param>
    /// <returns>The packet <c>53/10</c>.</returns>
    public static byte[] Bits(IEnumerable<ushort> keys)
    {
        var groups = new SortedDictionary<byte, byte>();
        foreach (var key in keys.Where(k => k is >= 1 and <= 255 * 8).Distinct())
        {
            var group = (byte)((key - 1) / 8 + 1);
            groups[group] = (byte)(groups.GetValueOrDefault(group) | 1 << ((key - 1) % 8));
        }
        var w = new PacketWriter(Action53.Code, Action53.BookBits);
        foreach (var (group, bits) in groups)
            w.U8(group).U8(bits);
        return w.ToArray();
    }

    /// <summary>Sends the player's whole book when they enter the world, if it is not empty.</summary>
    /// <param name="session">The player's session.</param>
    /// <param name="character">The player's character.</param>
    public static void SendOnLogin(GameSession session, Character character)
    {
        if (character.MonsterBook.Count > 0)
            session.Send(Bits(character.MonsterBook));
    }

    /// <summary>Adds the defeated monsters to the player's book and tells the client about each new entry.</summary>
    /// <param name="session">The player's session.</param>
    /// <param name="character">The player's character.</param>
    /// <param name="defeated">The monsters defeated in the battle.</param>
    public static void Record(GameSession session, Character character, IEnumerable<NpcRecord> defeated)
    {
        foreach (var npcId in Enter(character, defeated))
            session.Send(Added(npcId));
    }

    /// <summary>Adds the keys of the defeated monsters to the character's book.</summary>
    /// <param name="character">The player's character.</param>
    /// <param name="defeated">The monsters defeated in the battle; only monsters with a key count.</param>
    /// <returns>The Npc.dat ids of the monsters that were new to the book.</returns>
    public static List<ushort> Enter(Character character, IEnumerable<NpcRecord> defeated)
    {
        List<ushort> added = [];
        foreach (var npc in defeated.Where(n => n.IsMonster && n.Key != 0))
        {
            if (character.MonsterBook.Contains(npc.Key))
                continue;
            character.MonsterBook.Add(npc.Key);
            added.Add(npc.Id);
        }
        return added;
    }
}
