namespace NorthIsland.GameData.Skills;

/// <summary>One effect layer of a motion: a picture animation drawn along a path.</summary>
public sealed record MotionEffect
{
    /// <summary>The picture id of the animation.</summary>
    public int PictureId { get; init; }

    /// <summary>The points the layer moves along.</summary>
    public IReadOnlyList<MotionPoint> Path { get; init; } = [];

    /// <summary>The layer's speed along its path.</summary>
    public double Speed { get; init; }

    /// <summary>The delay in ms between animation frames.</summary>
    public int FrameDelay { get; init; }

    /// <summary>Byte at record offset 0x67; the code does not interpret it.</summary>
    public byte Param67 { get; init; }

    /// <summary>Byte at record offset 0x6B; the code does not interpret it.</summary>
    public byte Param6B { get; init; }

    /// <summary>Byte at record offset 0x6F; the code does not interpret it.</summary>
    public byte Param6F { get; init; }

    /// <summary>Byte at record offset 0x73; the code does not interpret it.</summary>
    public byte Param73 { get; init; }

    /// <summary>When the layer starts.</summary>
    public int Start { get; init; }

    /// <summary>Byte at record offset 0x7B; the code does not interpret it.</summary>
    public byte Param7B { get; init; }

    /// <summary>Byte at record offset 0x7F; the code does not interpret it.</summary>
    public byte Param7F { get; init; }

    /// <summary>Byte at record offset 0x83; the code does not interpret it.</summary>
    public byte Param83 { get; init; }

    /// <summary>Byte at record offset 0x87; the code does not interpret it.</summary>
    public byte Param87 { get; init; }

    /// <summary>The picture variant, 0..4.</summary>
    public byte Variant { get; init; }

    /// <summary>Float at record offset 0x8F; the code does not interpret it.</summary>
    public float Param8F { get; init; }
}
