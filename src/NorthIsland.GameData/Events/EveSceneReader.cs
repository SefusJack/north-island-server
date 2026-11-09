namespace NorthIsland.GameData.Events;

/// <summary>Decodes the scene blob of one map into an <see cref="EveScene"/>.</summary>
public static class EveSceneReader
{
    /// <summary>The only scene version accepted.</summary>
    public const ushort SupportedVersion = 112;

    /// <summary>The number of sections in a scene.</summary>
    public const int SectionCount = 11;

    /// <summary>Largest record index accepted per section (the sizes of the arrays that hold the records).</summary>
    private const int MaxNpcIndex = 100, MaxDoorIndex = 300, MaxMineIndex = 30, MaxGoodsIndex = 255,
        MaxEventIndex = 100, MaxGroupIndex = 150, MaxConveyIndex = 300, MaxInteractIndex = 50,
        MaxBattleIndex = 100, MaxLeadEventIndex = 50;

    /// <summary>Decodes a scene blob.</summary>
    /// <param name="blob">The scene bytes.</param>
    /// <returns>The scene.</returns>
    public static EveScene Read(ReadOnlySpan<byte> blob)
    {
        var r = new EveReader(blob);
        var headerMapId = r.U16();
        var version = r.U16();
        if (version != SupportedVersion)
            throw new EveFormatException($"Scene version {version}, the client only reads {SupportedVersion}.");

        var starts = new int[SectionCount];
        starts[0] = r.Position;
        var npcs = ReadSection(ref r, "Npc", MaxNpcIndex, ReadNpc);
        starts[1] = r.Position;
        var doors = ReadSection(ref r, "Door", MaxDoorIndex, ReadDoor);
        starts[2] = r.Position;
        var mines = ReadSection(ref r, "Mine", MaxMineIndex, ReadMine);
        starts[3] = r.Position;
        var goods = ReadSection(ref r, "Goods", MaxGoodsIndex, ReadGoods);
        starts[4] = r.Position;
        var events = ReadSection(ref r, "Eve", MaxEventIndex, ReadEvent);
        starts[5] = r.Position;
        var groups = ReadSection(ref r, "Group", MaxGroupIndex, ReadGroup);
        starts[6] = r.Position;
        var conveys = ReadSection(ref r, "Convey", MaxConveyIndex, ReadConvey);
        starts[7] = r.Position;
        var interacts = ReadSection(ref r, "Interact", MaxInteractIndex, ReadInteract);
        starts[8] = r.Position;
        var battles = ReadSection(ref r, "Battle", MaxBattleIndex, ReadBattle);
        starts[9] = r.Position;
        var leadEvents = ReadSection(ref r, "LeadEve", MaxLeadEventIndex, ReadEvent);
        starts[10] = r.Position;
        var leadGroups = ReadSection(ref r, "LeadGroup", MaxGroupIndex, ReadGroup);

        var offsets = new int[r.Remaining >= SectionCount * 4 ? SectionCount : 0];
        for (var i = 0; i < offsets.Length; i++)
            offsets[i] = r.I32();

        return new EveScene
        {
            HeaderMapId = headerMapId,
            Version = version,
            Npcs = npcs,
            Doors = doors,
            Mines = mines,
            Goods = goods,
            Events = events,
            Groups = groups,
            Conveys = conveys,
            Interacts = interacts,
            Battles = battles,
            LeadEvents = leadEvents,
            LeadGroups = leadGroups,
            SectionOffsets = offsets,
            SectionStarts = starts,
            TrailingBytes = r.Remaining,
        };
    }

    /// <summary>Reads one section: a count, then records each preceded by its index.</summary>
    /// <typeparam name="T">The record type.</typeparam>
    /// <param name="r">The reader.</param>
    /// <param name="what">The section name used in error messages.</param>
    /// <param name="maxIndex">The largest record index accepted.</param>
    /// <param name="read">Reads one record.</param>
    /// <returns>The records ordered by index; a repeated index keeps the last record.</returns>
    public static IReadOnlyList<T> ReadSection<T>(ref EveReader r, string what, int maxIndex, EveRecordReader<T> read)
    {
        int count = r.U16();
        var byIndex = new SortedDictionary<int, T>();
        for (var i = 0; i < count; i++)
        {
            int index = r.U16();
            r.Check(index <= maxIndex, $"{what} index {index} exceeds the client's array (max {maxIndex})");
            byIndex[index] = read(ref r, index);
        }
        return [.. byIndex.Values];
    }

    /// <summary>Reads the common start of a placed object into a copy of a prototype.</summary>
    /// <typeparam name="T">The placed object type.</typeparam>
    /// <param name="r">The reader.</param>
    /// <param name="index">The record index.</param>
    /// <param name="target">The prototype to copy and fill.</param>
    /// <returns>The filled copy.</returns>
    private static T ReadPlaced<T>(ref EveReader r, int index, T target) where T : EvePlacedObject => target with
    {
        Index = index,
        Name = r.ShortString(20),
        Flags = r.U8(),
        X = r.I32(),
        Y = r.I32(),
        EventIds = r.ByteList(10, "event id list"),
        Unknown48List = r.ByteList(10, "list @48"),
        Unknown60 = r.U8(),
    };

    /// <summary>Reads an NPC record.</summary>
    /// <param name="r">The reader.</param>
    /// <param name="index">The record index.</param>
    /// <returns>The NPC.</returns>
    public static EveNpc ReadNpc(ref EveReader r, int index)
    {
        var npc = ReadPlaced(ref r, index, new EveNpc());
        var npcId = r.I32();
        var direction = r.U8();
        var moveKind = r.U8();
        var unknown684 = r.U8();
        int pathCount = r.U8();
        r.Check(pathCount <= 50, $"NPC {index}: path of {pathCount} points (max 50)");
        var path = new EveNpcPathPoint[pathCount];
        for (var i = 0; i < pathCount; i++)
            path[i] = new EveNpcPathPoint(r.I32(), r.I32(), r.I32());
        var speed = r.U8();
        var unknown685 = r.U8();
        var unknown688 = r.I32();
        var unknown692 = r.I32();
        var unknown696 = r.U16();
        var unknown698 = r.U8();
        int itemCount = r.U8();
        var items = new List<EveNpcItem>(itemCount);
        for (var i = 0; i < itemCount; i++)
        {
            var raw = r.Bytes(92).ToArray();
            r.Check(raw[0] <= 10, $"NPC {index}: item slot {raw[0]} (max 10)");
            items.Add(new EveNpcItem(raw[0], raw[1], raw));
        }
        return npc with
        {
            NpcId = npcId,
            Direction = direction,
            MoveKind = moveKind,
            Unknown684 = unknown684,
            Path = path,
            Speed = speed,
            Unknown685 = unknown685,
            Unknown688 = unknown688,
            Unknown692 = unknown692,
            Unknown696 = unknown696,
            Unknown698Flags = unknown698,
            Items = items,
            Unknown1712 = r.U16(),
            Unknown1714 = r.U16(),
            Unknown1716 = r.I32(),
        };
    }

    /// <summary>Reads a door record.</summary>
    /// <param name="r">The reader.</param>
    /// <param name="index">The record index.</param>
    /// <returns>The door.</returns>
    public static EveDoor ReadDoor(ref EveReader r, int index)
    {
        var door = ReadPlaced(ref r, index, new EveDoor());
        return door with { X2 = r.I32(), Y2 = r.I32(), Kind = r.U8(), OffsetX = r.I32(), OffsetY = r.I32() };
    }

    /// <summary>Reads a mine record.</summary>
    /// <param name="r">The reader.</param>
    /// <param name="index">The record index.</param>
    /// <returns>The mine.</returns>
    public static EveMine ReadMine(ref EveReader r, int index)
    {
        var mine = ReadPlaced(ref r, index, new EveMine());
        return mine with { X2 = r.I32(), Y2 = r.I32(), Kind = r.U8(), Unknown73 = r.U8() };
    }

    /// <summary>Reads a goods record.</summary>
    /// <param name="r">The reader.</param>
    /// <param name="index">The record index.</param>
    /// <returns>The goods.</returns>
    public static EveGoods ReadGoods(ref EveReader r, int index)
    {
        var goods = ReadPlaced(ref r, index, new EveGoods());
        return goods with { Unknown64 = r.I32(), Unknown68 = r.U16(), Unknown70 = r.U8(), Unknown72 = r.I32() };
    }

    /// <summary>Reads an event record with its conditions and their results.</summary>
    /// <param name="r">The reader.</param>
    /// <param name="index">The record index.</param>
    /// <returns>The event.</returns>
    public static EveEvent ReadEvent(ref EveReader r, int index)
    {
        var flags = r.U8();
        var name = r.ShortString(20);
        int conditionCount = r.U8();
        r.Check(conditionCount <= 30, $"event {index}: {conditionCount} conditions (max 30)");
        var conditions = new List<EveCondition>(conditionCount);
        for (var c = 0; c < conditionCount; c++)
        {
            var record = ReadScriptRecord(ref r);
            int resultCount = r.U8();
            r.Check(resultCount <= 30, $"event {index} condition {c + 1}: {resultCount} results (max 30)");
            var results = new EveScriptRecord[resultCount];
            for (var i = 0; i < resultCount; i++)
                results[i] = ReadScriptRecord(ref r);
            conditions.Add(new EveCondition(record, results));
        }
        return new EveEvent { Index = index, Flags = flags, Name = name, Conditions = conditions };
    }

    /// <summary>Reads one 22-byte condition or result record.</summary>
    /// <param name="r">The reader.</param>
    /// <returns>The record.</returns>
    public static EveScriptRecord ReadScriptRecord(ref EveReader r) =>
        new(r.U8(), r.U8(), r.U16(), r.U16(), r.U16(), r.U8(), r.I32(), r.U8(), r.I32(), r.I32());

    /// <summary>Reads a group record with its members.</summary>
    /// <param name="r">The reader.</param>
    /// <param name="index">The record index.</param>
    /// <returns>The group.</returns>
    public static EveGroup ReadGroup(ref EveReader r, int index)
    {
        var name = r.ShortString(20);
        var unknown29 = r.U8();
        var unknown30 = r.U16();
        var unknown32 = r.U8();
        var unknown6 = r.U8();
        int memberCount = r.U16();
        r.Check(memberCount <= 30, $"group {index}: {memberCount} members (max 30)");
        var members = new EveGroupMember[memberCount];
        for (var i = 0; i < memberCount; i++)
            members[i] = new EveGroupMember(r.U16(), r.U8());
        return new EveGroup
        {
            Index = index,
            Name = name,
            Unknown29 = unknown29,
            Unknown30 = unknown30,
            Unknown32 = unknown32,
            Unknown6 = unknown6,
            Members = members,
        };
    }

    /// <summary>Reads a convey record.</summary>
    /// <param name="r">The reader.</param>
    /// <param name="index">The record index.</param>
    /// <returns>The convey.</returns>
    public static EveConvey ReadConvey(ref EveReader r, int index) => new()
    {
        Index = index,
        Name = r.ShortString(20),
        TargetMapId = r.U16(),
        X = r.I32(),
        Y = r.I32(),
        Direction = r.U8(),
        Unknown6 = r.U16(),
    };

    /// <summary>Reads an interaction record with its entries.</summary>
    /// <param name="r">The reader.</param>
    /// <param name="index">The record index.</param>
    /// <returns>The interaction.</returns>
    public static EveInteract ReadInteract(ref EveReader r, int index)
    {
        var name = r.ShortString(20);
        var kind = r.U8();
        var unknown6 = r.U16();
        int entryCount = r.U8();
        r.Check(entryCount <= 10, $"interact {index}: {entryCount} entries (max 10)");
        var entries = new EveInteractEntry[entryCount];
        for (var i = 0; i < entryCount; i++)
            entries[i] = new EveInteractEntry(r.U8(), r.U16(), r.U8(), r.U8());
        return new EveInteract { Index = index, Name = name, Kind = kind, Unknown6 = unknown6, Entries = entries };
    }

    /// <summary>Reads a battle record with both sides.</summary>
    /// <param name="r">The reader.</param>
    /// <param name="index">The record index.</param>
    /// <returns>The battle.</returns>
    public static EveBattle ReadBattle(ref EveReader r, int index)
    {
        var battle = ReadPlaced(ref r, index, new EveBattle());
        var unknown291 = r.U8();
        var unknown292 = r.U16();
        var unknown294 = r.U8();
        var left = ReadBattleSide(ref r, index, "left");
        var right = ReadBattleSide(ref r, index, "right");
        return battle with
        {
            Unknown291 = unknown291,
            Unknown292 = unknown292,
            Unknown294 = unknown294,
            Left = left,
            Right = right,
            Unknown295 = r.U8(),
        };
    }

    /// <summary>Reads the members of one side of a battle (at most 24).</summary>
    /// <param name="r">The reader.</param>
    /// <param name="index">The battle record index, for error messages.</param>
    /// <param name="side">The side name, for error messages.</param>
    /// <returns>The members.</returns>
    private static EveBattleMember[] ReadBattleSide(ref EveReader r, int index, string side)
    {
        int count = r.U8();
        r.Check(count <= 24, $"battle {index}: {count} {side} members (max 24)");
        var members = new EveBattleMember[count];
        for (var i = 0; i < count; i++)
            members[i] = new EveBattleMember(r.U16(), r.U8());
        return members;
    }
}
