using System.Buffers.Binary;

namespace NorthIsland.GameData.Skills;

/// <summary>The skill motion scripts of SkillData.MBTM by skill id, with the estimate of how long an action takes to play.</summary>
public sealed class SkillMotionTable
{
    /// <summary>The size in bytes of one index entry at the end of the file: u16 id, i32 position, i32 size.</summary>
    public const int IndexEntrySize = 10;

    /// <summary>The frame length in ms assumed by default.</summary>
    public const int DefaultFrameMs = 33;

    /// <summary>The number of frames a floating damage number takes to move along its arc.</summary>
    public const int NumberArcFrames = 35;

    /// <summary>The number of frames a rising heal number takes.</summary>
    public const int NumberRiseFrames = 101;

    /// <summary>The time in ms a knocked-off fighter takes to fly away.</summary>
    public const int KnockOffMs = 1250;

    /// <summary>The skill id Biochemical Battery, which is played as 15173 for some attackers.</summary>
    public const ushort BiochemicalBattery = 15172; // Biochemical Battery

    /// <summary>The motions by skill id.</summary>
    private readonly Dictionary<ushort, SkillMotion> _byId;

    /// <summary>Wraps the parsed motions.</summary>
    /// <param name="byId">The motions by skill id.</param>
    private SkillMotionTable(
        Dictionary<ushort, SkillMotion> byId) => _byId = byId;

    /// <summary>The number of motions.</summary>
    public int Count => _byId.Count;

    /// <summary>The motion of a skill id, or null when the file has none.</summary>
    /// <param name="id">The skill id.</param>
    public SkillMotion? this[ushort id] => _byId.GetValueOrDefault(id);

    /// <summary>All motions.</summary>
    public IEnumerable<SkillMotion> All => _byId.Values;

    /// <summary>Reads and parses a SkillData.MBTM file.</summary>
    /// <param name="path">Path of the file.</param>
    /// <returns>The parsed table.</returns>
    public static SkillMotionTable Open(string path) => Parse(File.ReadAllBytes(path));

    /// <summary>Parses the contents of a SkillData.MBTM file: the records, followed by an index and a u16 entry count at the end.</summary>
    /// <param name="data">The whole file.</param>
    /// <returns>The parsed table; the first record of a duplicate id is kept.</returns>
    public static SkillMotionTable Parse(ReadOnlySpan<byte> data)
    {
        if (data.Length < 2)
            throw new InvalidDataException("SkillData.MBTM: file too short");
        var count = BinaryPrimitives.ReadUInt16LittleEndian(data[^2..]);
        var indexStart = data.Length - 2 - count * IndexEntrySize;
        if (indexStart < 0)
            throw new InvalidDataException($"SkillData.MBTM: index of {count} entries does not fit");
        var byId = new Dictionary<ushort, SkillMotion>(count);
        for (var i = 0; i < count; i++)
        {
            var entry = data.Slice(indexStart + i * IndexEntrySize, IndexEntrySize);
            var id = BinaryPrimitives.ReadUInt16LittleEndian(entry);
            var position = BinaryPrimitives.ReadInt32LittleEndian(entry[2..]);
            var size = BinaryPrimitives.ReadInt32LittleEndian(entry[6..]);
            if (position < 0 || size < 0 || position + size > indexStart)
                throw new InvalidDataException($"SkillData.MBTM: record {id} lies outside the data");
            byId.TryAdd(id, ParseRecord(id, data.Slice(position, size)));
        }
        return new SkillMotionTable(byId);
    }

    /// <summary>Decodes one motion record.</summary>
    /// <param name="id">The skill id from the index entry.</param>
    /// <param name="record">The record's bytes.</param>
    /// <returns>The decoded motion.</returns>
    public static SkillMotion ParseRecord(ushort id, ReadOnlySpan<byte> record)
    {
        var r = new Reader(record);
        var unknown00 = r.U16();
        var unknown04 = r.U8();
        var approach = r.U8();
        var back = r.U8();
        var authored = r.I32();
        var jumpOut = r.F32();
        var jumpBack = r.F32();
        var unknown2B = r.U8();
        var reaction = r.U16();
        var hitAt = r.I32();
        var numberMode = r.U8();

        var hits = new MotionHit[r.U8()];
        for (var i = 0; i < hits.Length; i++)
            hits[i] = new MotionHit(r.I32(), r.U8());

        var poses = new MotionPose[r.U8()];
        for (var i = 0; i < poses.Length; i++)
            poses[i] = new MotionPose(r.U8(), r.U8(), r.F64(), r.I32());

        var pointCount = r.U8();
        var stored = new MotionPoint[poses.Length];
        for (var i = 0; i < stored.Length; i++)
            stored[i] = r.Point();
        var points = stored.Take(Math.Min((int)pointCount, stored.Length)).ToArray();

        var effects = new MotionEffect[r.U8()];
        for (var i = 0; i < effects.Length; i++)
        {
            var picture = r.I32();
            var path = new MotionPoint[r.U8()];
            for (var j = 0; j < path.Length; j++)
                path[j] = r.Point();
            effects[i] = new MotionEffect
            {
                PictureId = picture, Path = path, Speed = r.F64(), FrameDelay = r.I32(),
                Param67 = r.U8(), Param6B = r.U8(), Param6F = r.U8(), Param73 = r.U8(), Start = r.I32(),
                Param7B = r.U8(), Param7F = r.U8(), Param83 = r.U8(), Param87 = r.U8(), Variant = r.U8(), Param8F = r.F32(),
            };
        }
        var order = new byte[effects.Length + 1];
        for (var i = 0; i < order.Length; i++)
            order[i] = r.U8();

        var sounds = new MotionSound[r.U8()];
        for (var i = 0; i < sounds.Length; i++)
            sounds[i] = new MotionSound(r.U16(), r.I32(), r.U8());

        var cameraMode = r.U8();
        var camera = new MotionCameraStep[r.U8()];
        for (var i = 0; i < camera.Length; i++)
            camera[i] = new MotionCameraStep(r.Point(), r.F64(), r.I32(), r.U8());

        var groundMode = r.U8();
        var groundPath = new MotionPoint[r.U8()];
        for (var i = 0; i < groundPath.Length; i++)
            groundPath[i] = r.Point();
        var groundRanges = new (byte, byte)[r.U8()];
        for (var i = 0; i < groundRanges.Length; i++)
            groundRanges[i] = (r.U8(), r.U8());
        var screen = new MotionScreenEffect[r.U8()];
        for (var i = 0; i < screen.Length; i++)
            screen[i] = new MotionScreenEffect(r.U8(), r.I32(), r.I32());
        var tail = r.Bytes(72 + 24 + 6 + 6 + 60 + 24 + 6);

        return new SkillMotion
        {
            Id = id, Unknown00 = unknown00, Unknown04 = unknown04, ApproachMode = approach, ReturnMode = back,
            AuthoredLengthMs = authored, JumpScaleOut = jumpOut, JumpScaleBack = jumpBack, Unknown2B = unknown2B,
            ReactionMotionId = reaction, HitAt = hitAt, NumberMode = numberMode, Hits = hits, Poses = poses, Points = points, PathPointCount = pointCount,
            Effects = effects, EffectOrder = order, Sounds = sounds, CameraMode = cameraMode, CameraSteps = camera,
            GroundMode = groundMode, GroundPath = groundPath, GroundRanges = groundRanges, ScreenEffects = screen,
            SoundTail = tail,
        };
    }

    /// <summary>The screen position of a battle grid cell.</summary>
    /// <param name="cell">The cell.</param>
    /// <returns>The X and Y position in pixels.</returns>
    public static (int X, int Y) CellPosition(BattleCell cell)
    {
        var x = cell.Row switch
        {
            1 => 210 - 25 * cell.Col, // row 1: 210 minus 25 per column
            2 => 280 - 25 * cell.Col, // row 2: 280 minus 25 per column
            3 => 540 + 25 * cell.Col, // row 3: 540 plus 25 per column
            _ => 610 + 25 * cell.Col, // any other row: 610 plus 25 per column
        };
        return (x, 63 * cell.Col + 266);
    }

    /// <summary>The side of the field a cell is on.</summary>
    /// <param name="cell">The cell.</param>
    /// <returns>1 for rows 3 and 4, 2 for rows 1 and 2.</returns>
    public static int SideOf(BattleCell cell) => cell.Row >= 3 ? 1 : 2;

    /// <summary>
    /// Estimates the time the client takes to play one skill action, by parts, from the skill's motion and the cells involved; with <paramref
    /// name="ticks"/> it also runs the tick simulation and keeps the longer of the two estimates.
    /// </summary>
    /// <param name="skillId">The skill id sent in the action.</param>
    /// <param name="attacker">The attacker's cell.</param>
    /// <param name="target">The target's cell.</param>
    /// <param name="skill">The skill's Skill.dat record, or null for an attack on the other side.</param>
    /// <param name="showsNumbers">Whether floating damage numbers are shown.</param>
    /// <param name="targetDefends">Whether the target guards and plays the defend motion instead of the reaction motion.</param>
    /// <param name="frameMs">The frame length in ms.</param>
    /// <param name="ticks">The tick costs for the tick simulation, or null for the frame-based estimate only.</param>
    /// <param name="hitFighters">How many fighters are hit.</param>
    /// <returns>The timing of the action; all zeros when the skill has no motion.</returns>
    public ActionTiming EstimateAction(
        ushort skillId,
        BattleCell attacker,
        BattleCell target,
        SkillRecord? skill = null,
        bool showsNumbers = true,
        bool targetDefends = false,
        int frameMs = DefaultFrameMs,
        ClientTickTiming? ticks = null,
        int hitFighters = 1)
    {
        var motion = this[skillId];
        if (motion is null)
            return default;
        double frame = frameMs;

        var home = CellPosition(attacker);
        var aim = CellPosition(target);
        var side = SideOf(attacker);
        int offX, offY;
        if (skill?.Type == 12)
            (offX, offY) = (0, 0);
        else if (skill is { TargetType: 1 or 3 or 5 })
            (offX, offY) = skill.Type is 1 or 2 ? (aim.X < home.X ? aim.X - home.X : Math.Abs(home.X - aim.X), home.Y - aim.Y) : (0, 0);
        else if (SideOf(target) == side)
            (offX, offY) = (Math.Abs(home.X - aim.X), home.Y - aim.Y);
        else
            (offX, offY) = (Math.Abs(home.X - aim.X) - 570, home.Y - aim.Y);

        var run = RunPath(motion, home, offX, offY, side, frame);
        var reactionMotion = motion.ReactionMotionId != 0 ? this[targetDefends ? SkillTable.Defense : motion.ReactionMotionId] : null;
        var tickRun = ticks is null ? (ClientTickResult?)null
            : ClientTickModel.Run(motion, home, offX, offY, side, reactionMotion, CellPosition(target), SideOf(target), Math.Max(1, hitFighters), ticks);
        if (tickRun is { } tr0)
            run = run with { EndMs = tr0.PathEndMs, IndexMs = tr0.IndexMs };
        var attackerMs = run.EndMs + Move(run.End, home, WalkSpeed(motion.ReturnMode), frame) + 2 * frame;
        if (ticks is not null && motion.CameraSteps.Count > 0)
            attackerMs += ticks.CameraReturnMs;

        var hitMs = At(motion.HitAt, run);
        var reactionId = targetDefends ? SkillTable.Defense : motion.ReactionMotionId;
        var reactionMs = 0.0;
        if (motion.ReactionMotionId != 0 && this[reactionId] is { } reaction)
        {
            var targetHome = CellPosition(target);
            var react = RunPath(reaction, targetHome, 0, 0, SideOf(target), frame);
            var reactEnd = tickRun is { ReactionPathEndMs: > 0 } tr1 ? tr1.ReactionPathEndMs : hitMs + react.EndMs;
            reactionMs = reactEnd + Move(react.End, targetHome, WalkSpeed(reaction.ReturnMode), frame) + frame;
        }

        var numbersMs = 0.0;
        if (showsNumbers)
        {
            var start = hitMs;
            if (motion.Hits.Count > 0)
                start = motion.NumberMode == 2
                    ? motion.Hits.Max(h => Math.Max(hitMs, At(h.At, run)))
                    : Math.Max(hitMs, At(motion.Hits[0].At, run));
            var rise = motion.NumberMode == 0 && skill?.Type is 6 or 7 or 13;
            numbersMs = start + (rise ? NumberRiseFrames : NumberArcFrames) * frame;
        }

        var total = Math.Max(attackerMs, Math.Max(reactionMs, numbersMs));
        var timing = new ActionTiming((int)Math.Ceiling(attackerMs), (int)Math.Ceiling(hitMs), (int)Math.Ceiling(reactionMs),
            (int)Math.Ceiling(numbersMs), (int)Math.Ceiling(total));
        if (ticks is null)
            return timing;
        var closed = EstimateAction(skillId, attacker, target, skill, showsNumbers, targetDefends, frameMs);
        return new ActionTiming(Math.Max(timing.AttackerMs, closed.AttackerMs), timing.HitMs, Math.Max(timing.ReactionMs, closed.ReactionMs),
            Math.Max(timing.NumbersMs, closed.NumbersMs), Math.Max(timing.TotalMs, closed.TotalMs));
    }

    /// <summary>The total of <see cref="EstimateAction"/> in ms.</summary>
    /// <param name="skillId">The skill id sent in the action.</param>
    /// <param name="attacker">The attacker's cell.</param>
    /// <param name="target">The target's cell.</param>
    /// <param name="skill">The skill's Skill.dat record, or null for an attack on the other side.</param>
    /// <param name="showsNumbers">Whether floating damage numbers are shown.</param>
    /// <param name="targetDefends">Whether the target guards.</param>
    /// <param name="frameMs">The frame length in ms.</param>
    /// <param name="ticks">The tick costs, or null.</param>
    /// <param name="hitFighters">How many fighters are hit.</param>
    /// <returns>The total time in ms.</returns>
    public int EstimateActionMs(
        ushort skillId,
        BattleCell attacker,
        BattleCell target,
        SkillRecord? skill = null,
        bool showsNumbers = true,
        bool targetDefends = false,
        int frameMs = DefaultFrameMs,
        ClientTickTiming? ticks = null,
        int hitFighters = 1) =>
            EstimateAction(skillId, attacker, target, skill, showsNumbers, targetDefends, frameMs, ticks, hitFighters).TotalMs;

    /// <summary>Finds the longest total time of a skill over all attacker and target cells.</summary>
    /// <param name="skillId">The skill id.</param>
    /// <param name="skill">The skill's Skill.dat record, or null.</param>
    /// <param name="showsNumbers">Whether floating damage numbers are shown.</param>
    /// <param name="frameMs">The frame length in ms.</param>
    /// <returns>The longest total time in ms.</returns>
    public int EstimateWorstCaseMs(ushort skillId, SkillRecord? skill = null, bool showsNumbers = true, int frameMs = DefaultFrameMs)
    {
        var self = skill is not null && (skill.Type == 12 || skill.TargetType is 1 or 3 or 5);
        var worst = 0;
        for (var ar = 1; ar <= 4; ar++)
            for (var ac = 1; ac <= 4; ac++)
            {
                var a = new BattleCell(ar, ac);
                if (self)
                {
                    worst = Math.Max(worst, EstimateActionMs(skillId, a, a, skill, showsNumbers, false, frameMs));
                    continue;
                }
                for (var tr = ar >= 3 ? 1 : 3; tr <= (ar >= 3 ? 2 : 4); tr++)
                    for (var tc = 1; tc <= 4; tc++)
                        worst = Math.Max(worst, EstimateActionMs(skillId, a, new BattleCell(tr, tc), skill, showsNumbers, false, frameMs));
            }
        return worst;
    }

    /// <summary>The result of walking a motion's path.</summary>
    /// <param name="EndMs">When the last point and pose are done.</param>
    /// <param name="End">The position at the end.</param>
    /// <param name="IndexMs">When each pose index is reached, indexed by the pose index.</param>
    private sealed record PathRun(
        double EndMs,
        (int X, int Y) End,
        double[] IndexMs);

    /// <summary>Walks a motion's points frame by frame and finds when each pose index is reached and when the path ends.</summary>
    /// <param name="m">The motion.</param>
    /// <param name="home">Screen position of the fighter's cell.</param>
    /// <param name="offX">X offset of the path anchor.</param>
    /// <param name="offY">Y offset of the path anchor.</param>
    /// <param name="side">Side of the fighter's cell; 1 mirrors the X axis of the path.</param>
    /// <param name="frame">The frame length in ms.</param>
    /// <returns>The end time, the end position and the time of each pose index.</returns>
    private static PathRun RunPath(
        SkillMotion m,
        (int X, int Y) home,
        int offX,
        int offY,
        int side,
        double frame)
    {
        var index = new double[Math.Max(m.Poses.Count, m.Points.Count) + 2];
        var t = 0.0;
        var pos = home;
        var steps = Math.Max(m.Points.Count, m.Poses.Count);
        for (var k = 1; k <= steps; k++)
        {
            if (k <= m.Points.Count)
            {
                var p = m.Points[k - 1];
                var next = side == 1 ? (home.X - p.X - offX, p.Y + home.Y - offY) : (home.X + p.X + offX, p.Y + home.Y - offY);
                var speed = k == 1 ? WalkSpeed(m.ApproachMode) : k <= m.Poses.Count ? m.Poses[k - 1].Speed : 0.6;
                t += Move(pos, next, speed, frame);
                pos = next;
            }
            if (k == 1)
                index[1] = t;
            var hold = k <= m.Poses.Count ? m.Poses[k - 1].HoldMs : 0;
            if (hold > 0)
                t += Math.Ceiling(hold / frame) * frame;
            index[k + 1] = t;
        }
        return new PathRun(t, pos, index);
    }

    /// <summary>The time of a value that is a pose index when 1..100 and ms since the action began otherwise.</summary>
    /// <param name="value">The pose index or ms.</param>
    /// <param name="run">The path run that gives the time of each pose index.</param>
    /// <returns>The time in ms.</returns>
    private static double At(int value, PathRun run)
    {
        if (value is > 0 and < 101)
            return value < run.IndexMs.Length ? run.IndexMs[value] : run.EndMs;
        return Math.Max(0, value);
    }

    /// <summary>The walking speed of an approach or return mode.</summary>
    /// <param name="mode">0 for running, 1 for jumping.</param>
    /// <returns>The speed in px per ms.</returns>
    private static double WalkSpeed(byte mode) => mode == 1 ? 0.8 : 0.6;

    /// <summary>The time in ms to move between two points, rounded up to whole frames; an empty move takes one frame.</summary>
    /// <param name="from">The start point.</param>
    /// <param name="to">The end point.</param>
    /// <param name="speed">The speed in px per ms.</param>
    /// <param name="frame">The frame length in ms.</param>
    /// <returns>The time in ms.</returns>
    private static double Move((int X, int Y) from, (int X, int Y) to, double speed, double frame)
    {
        var distance = Math.Max(Math.Abs(to.X - from.X), Math.Abs(to.Y - from.Y));
        if (distance == 0 || speed <= 0)
            return frame;
        return Math.Max(1, Math.Ceiling(distance / speed / frame)) * frame;
    }

    /// <summary>A reader of the little-endian fields of one motion record.</summary>
    /// <param name="data">The record's bytes.</param>
    private ref struct Reader(
        ReadOnlySpan<byte> data)
    {
        /// <summary>The record's bytes.</summary>
        private readonly ReadOnlySpan<byte> _data = data;

        /// <summary>The read position.</summary>
        private int _at;

        /// <summary>Takes the next bytes and moves the position on; throws when the record ends early.</summary>
        /// <param name="n">The number of bytes.</param>
        /// <returns>The bytes.</returns>
        private ReadOnlySpan<byte> Take(int n)
        {
            if (_at + n > _data.Length)
                throw new InvalidDataException("SkillData.MBTM: record ends early");
            var s = _data.Slice(_at, n);
            _at += n;
            return s;
        }

        /// <summary>Reads a u8.</summary>
        /// <returns>The value.</returns>
        public byte U8() => Take(1)[0];

        /// <summary>Reads a little-endian u16.</summary>
        /// <returns>The value.</returns>
        public ushort U16() => BinaryPrimitives.ReadUInt16LittleEndian(Take(2));

        /// <summary>Reads a little-endian i32.</summary>
        /// <returns>The value.</returns>
        public int I32() => BinaryPrimitives.ReadInt32LittleEndian(Take(4));

        /// <summary>Reads a little-endian f32.</summary>
        /// <returns>The value.</returns>
        public float F32() => BinaryPrimitives.ReadSingleLittleEndian(Take(4));

        /// <summary>Reads a little-endian f64.</summary>
        /// <returns>The value.</returns>
        public double F64() => BinaryPrimitives.ReadDoubleLittleEndian(Take(8));

        /// <summary>Reads a point stored as two i32.</summary>
        /// <returns>The point.</returns>
        public MotionPoint Point()
        {
            var s = Take(8);
            return new MotionPoint(BinaryPrimitives.ReadInt32LittleEndian(s), BinaryPrimitives.ReadInt32LittleEndian(s[4..]));
        }

        /// <summary>Reads a copy of the next bytes.</summary>
        /// <param name="n">The number of bytes.</param>
        /// <returns>The bytes.</returns>
        public ReadOnlyMemory<byte> Bytes(int n) => Take(n).ToArray();
    }
}
