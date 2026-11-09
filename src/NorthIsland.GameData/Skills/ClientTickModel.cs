namespace NorthIsland.GameData.Skills;

/// <summary>
/// Replays the original client's tick loop for one skill action to find when the attacker's path, the hit and the targets' reaction end.
/// </summary>
public static class ClientTickModel
{

    /// <summary>Runs the tick simulation once for each vertical blank phase and averages the times.</summary>
    /// <param name="attacker">The motion the attacker plays.</param>
    /// <param name="home">Screen position of the attacker's cell.</param>
    /// <param name="offX">X offset of the path anchor, added to every point.</param>
    /// <param name="offY">Y offset of the path anchor, subtracted from every point.</param>
    /// <param name="side">Side of the attacker's cell; 1 mirrors the X axis of the path.</param>
    /// <param name="reaction">The motion every hit target plays, or null when the targets play none.</param>
    /// <param name="targetHome">Screen position of the target's cell.</param>
    /// <param name="targetSide">Side of the target's cell.</param>
    /// <param name="hitFighters">How many fighters are hit; each plays its own copy of the reaction motion.</param>
    /// <param name="timing">The costs of one tick, and the number of vertical blank phases to average over.</param>
    /// <returns>The path end, hit and reaction end times in ms and the time each pose index is reached, averaged over the phases.</returns>
    public static ClientTickResult Run(
        SkillMotion attacker,
        (int X, int Y) home,
        int offX,
        int offY,
        int side,
        SkillMotion? reaction,
        (int X, int Y) targetHome,
        int targetSide,
        int hitFighters,
        ClientTickTiming timing)
    {
        var phases = Math.Max(1, timing.Phases);
        double path = 0, hit = 0, react = 0;
        var index = new double[attacker.Poses.Count + 2];
        for (var i = 0; i < phases; i++)
        {
            var r = RunOnce(attacker, home, offX, offY, side, reaction, targetHome, targetSide, hitFighters, timing, i * timing.VerticalBlankMs / phases);
            path += r.PathEndMs;
            hit += r.HitMs;
            react += r.ReactionPathEndMs;
            for (var k = 0; k < index.Length; k++)
                index[k] += r.IndexMs[k] / phases;
        }
        return new ClientTickResult(path / phases, hit / phases, react / phases, index);
    }

    /// <summary>Simulates the action tick by tick from time 0 for one position of the vertical blank relative to the action start.</summary>
    /// <param name="a">The motion the attacker plays.</param>
    /// <param name="home">Screen position of the attacker's cell.</param>
    /// <param name="offX">X offset of the path anchor, added to every point.</param>
    /// <param name="offY">Y offset of the path anchor, subtracted from every point.</param>
    /// <param name="side">Side of the attacker's cell; 1 mirrors the X axis of the path.</param>
    /// <param name="reaction">The motion every hit target plays, or null when the targets play none.</param>
    /// <param name="targetHome">Screen position of the target's cell.</param>
    /// <param name="targetSide">Side of the target's cell.</param>
    /// <param name="hitFighters">How many fighters are hit.</param>
    /// <param name="t">The costs of one tick.</param>
    /// <param name="vblankPhase">Offset in ms of the vertical blank from the start of the action.</param>
    /// <returns>The path end, hit and reaction end times in ms and the time each pose index is reached.</returns>
    private static ClientTickResult RunOnce(
        SkillMotion a,
        (int X, int Y) home,
        int offX,
        int offY,
        int side,
        SkillMotion? reaction,
        (int X, int Y) targetHome,
        int targetSide,
        int hitFighters,
        ClientTickTiming t,
        double vblankPhase)
    {
        var clock = new Clock();
        var att = new Motion(a, home, offX, offY, side, clock);
        var hits = new List<Motion>();
        var hitStaged = false;
        double hitAt = 0, now = 0, attEnd = 0, reactEnd = 0;
        var index = new double[a.Poses.Count + 2];
        var reached = 0;
        for (var tick = 0; tick < 1500; tick++)
        {
            clock.Now = now;
            var tickStart = now;
            if (!att.Finished)
            {
                now = att.Tick(t, now, clock);
                if (att.Finished)
                    attEnd = now;
                for (; reached < att.PointIndex && reached + 1 < index.Length; reached++)
                    index[reached + 1] = now;
            }
            var due = a.HitAt is > 0 and < 101 ? att.PointIndex >= a.HitAt && att.SpeedMode == 1 : now >= a.HitAt;
            if (!hitStaged && due)
            {
                hitStaged = true;
                hitAt = now;
                if (reaction is not null)
                    for (var i = 0; i < hitFighters; i++)
                        hits.Add(new Motion(reaction, targetHome, 0, 0, targetSide, clock));
            }
            foreach (var h in hits.Where(h => !h.Finished))
            {
                now = h.Tick(t, now, clock);
                if (h.Finished)
                    reactEnd = Math.Max(reactEnd, now);
            }
            if (att.Finished && hits.All(h => h.Finished))
                break;
            now += t.DrawMs;
            if (t.VerticalBlankMs > 0)
                now = Math.Ceiling((now - vblankPhase) / t.VerticalBlankMs) * t.VerticalBlankMs + vblankPhase;
            now = now % t.BeatMs != 0 ? (Math.Floor(now / t.BeatMs) + 1) * t.BeatMs : now > tickStart ? now : now + t.BeatMs;
        }
        for (var k = reached + 1; k < index.Length; k++)
            index[k] = attEnd;
        return new ClientTickResult(attEnd, hitStaged ? hitAt : attEnd, hits.Count > 0 ? reactEnd : 0, index);
    }

    /// <summary>The simulated millisecond clock shared by the motions of one run.</summary>
    private sealed class Clock
    {
        /// <summary>The simulated time in ms since the action began.</summary>
        public double Now;

        /// <summary>The time in whole ms, as the client's tick counter would read it.</summary>
        public double TickCount => Math.Floor(Now);
    }

    /// <summary>One fighter's walk along its motion's points and poses, advanced one tick at a time.</summary>
    private sealed class Motion
    {
        /// <summary>The motion script being played.</summary>
        private readonly SkillMotion _m;

        /// <summary>The path points in screen coordinates, 1-based; unused slots hold the home position.</summary>
        private readonly (int X, int Y)[] _pts;

        /// <summary>The number of path points to walk.</summary>
        private readonly int _pathCount;

        /// <summary>The number of poses of the motion.</summary>
        private readonly int _last;

        /// <summary>The current position on screen.</summary>
        private (int X, int Y) _pos;

        /// <summary>The number of path points reached so far; the point being walked to is the next one.</summary>
        private int _pathIndex;

        /// <summary>
        /// Progress flags: the first point was reached, a pose hold is running, all points are reached, all poses are done, and the length of the
        /// current segment is measured.
        /// </summary>
        private bool _pathAdvanced, _timed, _pathEnd, _pathDone, _segStarted;

        /// <summary>
        /// Times in ms: the hold time to leave out of the next step, the tick of the last point reached or pose change, and the tick the last step
        /// was measured from.
        /// </summary>
        private double _paused, _pathStepTick, _moveStart;

        /// <summary>The X and Y length of the current segment, used to share a step between the two axes.</summary>
        private int _segDx, _segDy;

        /// <summary>The end point of the current segment.</summary>
        private (int X, int Y) _target;

        /// <summary>The current pose index, 1-based; 0 until the first point is reached.</summary>
        public int PointIndex { get; private set; }

        /// <summary>0 while walking to the first point, 1 once the poses set the speed.</summary>
        public int SpeedMode { get; private set; }

        /// <summary>Whether every point is reached and every pose is done.</summary>
        public bool Finished => _pathEnd && _pathDone;

        /// <summary>Lays out the path points in screen coordinates around the home position; side 1 mirrors X.</summary>
        /// <param name="m">The motion to play.</param>
        /// <param name="home">Screen position of the fighter's cell.</param>
        /// <param name="offX">X offset of the path anchor.</param>
        /// <param name="offY">Y offset of the path anchor.</param>
        /// <param name="side">Side of the fighter's cell; 1 mirrors the X axis of the path.</param>
        /// <param name="clock">The shared clock the first step is measured from.</param>
        public Motion(
            SkillMotion m,
            (int X, int Y) home,
            int offX,
            int offY,
            int side,
            Clock clock)
        {
            _m = m;
            _pathCount = Math.Max(m.PathPointCount, m.Points.Count);
            _last = m.Poses.Count;
            _pts = new (int X, int Y)[Math.Max(_pathCount, _last) + 2];
            Array.Fill(_pts, home);
            for (var i = 1; i <= Math.Min(_pathCount, m.Points.Count); i++)
            {
                var p = m.Points[i - 1];
                _pts[i] = side == 1 ? (home.X - p.X - offX, p.Y + home.Y - offY) : (home.X + p.X + offX, p.Y + home.Y - offY);
            }
            _pos = home;
            _moveStart = clock.TickCount;
            _pathEnd = _pathCount == 0;
            _pathDone = _last == 0;
        }

        /// <summary>Whether this tick sleeps: the fighter holds on a point in speed mode 1 and no pause is pending.</summary>
        /// <returns>True when the tick sleeps.</returns>
        private bool Sleeps() =>
            _pathCount > 0 && _pathIndex < _pathCount && SpeedMode == 1 && _paused == 0 && _pts[_pathIndex] == _pts[_pathIndex + 1];

        /// <summary>Runs one tick: the hold sleep when it applies, then the path step and, in speed mode 1, the pose change.</summary>
        /// <param name="t">The costs of one tick.</param>
        /// <param name="now">The simulated time in ms at the start of the tick.</param>
        /// <param name="clock">The shared clock, moved forward by the sleep.</param>
        /// <returns>The simulated time in ms after the tick's sleep.</returns>
        public double Tick(ClientTickTiming t, double now, Clock clock)
        {
            if (Sleeps())
            {
                now += t.HoldSleepMs + t.SleepOvershootMs;
                clock.Now = now;
            }
            AdvancePath(clock);
            if (SpeedMode == 1)
                AdvancePoint(clock);
            return now;
        }

        /// <summary>
        /// Moves the fighter toward the current target by the distance covered since the last step, along the longer axis of the segment.
        /// </summary>
        /// <param name="clock">The shared clock.</param>
        /// <returns>True when the target is reached.</returns>
        private bool StepToward(Clock clock)
        {
            double el;
            if (_paused > 0)
            {
                el = Math.Max(0, clock.TickCount - _moveStart - _paused);
                _paused = 0;
            }
            else
                el = clock.TickCount - _moveStart;
            _moveStart = clock.TickCount;
            var step = SpeedMode == 1
                ? (int)Math.Round(el * _m.Poses[Math.Clamp(PointIndex, 1, _last) - 1].Speed)
                : (int)Math.Round((_m.ApproachMode == 1 ? 0.8 : 0.6) * el);
            var (tx, ty) = _target;
            static int Move(int from, int to, int s) => from == to ? from : from < to ? Math.Min(from + s, to) : Math.Max(from - s, to);
            int nx, ny;
            if (_segDx >= _segDy)
            {
                nx = Move(_pos.X, tx, step);
                ny = Move(_pos.Y, ty, (int)Math.Round(_segDy / (_segDx + 0.0001) * step));
            }
            else
            {
                nx = Move(_pos.X, tx, (int)Math.Round(_segDx / (_segDy + 0.0001) * step));
                ny = Move(_pos.Y, ty, step);
            }
            _pos = (nx, ny);
            return _pos == _target;
        }

        /// <summary>
        /// Steps toward the next path point; on the first arrival it starts speed mode 1, and past the last point it ends the path.
        /// </summary>
        /// <param name="clock">The shared clock.</param>
        private void AdvancePath(Clock clock)
        {
            if (_pathCount > 0 && _pathIndex < _pathCount && !(SpeedMode == 1 && _timed))
            {
                var next = _pts[_pathIndex + 1];
                if (!_segStarted)
                {
                    _segDx = Math.Abs(_pos.X - next.X);
                    _segDy = Math.Abs(_pos.Y - next.Y);
                    _target = next;
                    _segStarted = true;
                }
                if (StepToward(clock))
                {
                    _pathStepTick = clock.TickCount;
                    _timed = true;
                    _segStarted = false;
                    if (!_pathAdvanced)
                    {
                        _pathAdvanced = true;
                        SpeedMode = 1;
                        PointIndex = 1;
                        _pathIndex = 0;
                        _pathDone = _last == 0;
                        _pathEnd = false;
                        _moveStart = clock.TickCount;
                    }
                    _pathIndex++;
                }
            }
            if (_pathIndex >= _pathCount)
                _pathEnd = true;
        }

        /// <summary>Ends the pose hold once its time has passed and moves to the next pose.</summary>
        /// <param name="clock">The shared clock.</param>
        private void AdvancePoint(Clock clock)
        {
            if (PointIndex != _pathIndex || PointIndex < 1 || PointIndex > _last)
                return;
            if (clock.TickCount - _pathStepTick >= _m.Poses[PointIndex - 1].HoldMs)
            {
                if (SpeedMode != 2)
                    _paused = clock.TickCount - _pathStepTick;
                _pathStepTick = clock.TickCount;
                PointIndex++;
                _timed = false;
                if (PointIndex > _last)
                {
                    PointIndex = _pathEnd ? _last + 1 : _last;
                    _pathDone = true;
                }
            }
        }
    }
}
