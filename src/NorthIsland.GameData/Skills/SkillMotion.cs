namespace NorthIsland.GameData.Skills;

/// <summary>A skill's motion script from SkillData.MBTM: the path the attacker walks, the hit timing and the effects, camera and sounds.</summary>
public sealed record SkillMotion
{
    /// <summary>The skill id of the motion.</summary>
    public required ushort Id { get; init; }

    /// <summary>The u16 at the start of the record; the code does not interpret it.</summary>
    public ushort Unknown00 { get; init; }

    /// <summary>A u8 near the start of the record; the code does not interpret it.</summary>
    public byte Unknown04 { get; init; }

    /// <summary>How the attacker walks to the first point: 0 runs, 1 jumps.</summary>
    public byte ApproachMode { get; init; }

    /// <summary>How the attacker walks back home: 0 runs, 1 jumps.</summary>
    public byte ReturnMode { get; init; }

    /// <summary>The motion's length in ms as stored in the file; the timing estimate does not use it.</summary>
    public int AuthoredLengthMs { get; init; }

    /// <summary>The jump height scale on the way out.</summary>
    public float JumpScaleOut { get; init; }

    /// <summary>The jump height scale on the way back.</summary>
    public float JumpScaleBack { get; init; }

    /// <summary>A u8 in the record after the jump scales; the code does not interpret it.</summary>
    public byte Unknown2B { get; init; }

    /// <summary>The id of the motion every hit target plays; 0 when the targets play none.</summary>
    public ushort ReactionMotionId { get; init; }

    /// <summary>When the targets are hit: a pose index 1..100 of the attacker, otherwise ms since the action began.</summary>
    public int HitAt { get; init; }

    /// <summary>
    /// How damage numbers are timed: 0 one total at the first hit, 1 every hit at the first hit's time, 2 each hit at its own time.
    /// </summary>
    public byte NumberMode { get; init; }

    /// <summary>The damage-number hits.</summary>
    public IReadOnlyList<MotionHit> Hits { get; init; } = [];

    /// <summary>The poses, one per path point.</summary>
    public IReadOnlyList<MotionPose> Poses { get; init; } = [];

    /// <summary>The path points the attacker walks, relative to the anchor.</summary>
    public IReadOnlyList<MotionPoint> Points { get; init; } = [];

    /// <summary>The number of path points the client walks, as stored; <see cref="Points"/> holds that many.</summary>
    public int PathPointCount { get; init; }

    /// <summary>The effect layers.</summary>
    public IReadOnlyList<MotionEffect> Effects { get; init; } = [];

    /// <summary>The drawing order of the layers around the fighter; one more entry than there are effects.</summary>
    public IReadOnlyList<byte> EffectOrder { get; init; } = [];

    /// <summary>The sounds of the motion.</summary>
    public IReadOnlyList<MotionSound> Sounds { get; init; } = [];

    /// <summary>The camera mode: 0 follows the attacker, 1 jumps between the steps, 2 moves along a camera path.</summary>
    public byte CameraMode { get; init; }

    /// <summary>The camera script steps.</summary>
    public IReadOnlyList<MotionCameraStep> CameraSteps { get; init; } = [];

    /// <summary>A mode that selects how the fighter's ground line is placed.</summary>
    public byte GroundMode { get; init; }

    /// <summary>The points used by ground mode 3.</summary>
    public IReadOnlyList<MotionPoint> GroundPath { get; init; } = [];

    /// <summary>The point ranges used by ground mode 2.</summary>
    public IReadOnlyList<(byte From, byte To)> GroundRanges { get; init; } = [];

    /// <summary>The screen effect windows.</summary>
    public IReadOnlyList<MotionScreenEffect> ScreenEffects { get; init; } = [];

    /// <summary>The fixed 198-byte tail of the record, kept as read (sound tables); the code does not interpret it.</summary>
    public ReadOnlyMemory<byte> SoundTail { get; init; }
}
