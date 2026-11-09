namespace NorthIsland.Server.Data;

/// <summary>A manufacturing job running (or paused) at a work station of the character's house.</summary>
public sealed class MakeJob
{
    /// <summary>Job slot, 1-based; unique per character.</summary>
    public byte Slot { get; set; }

    /// <summary>Index of the recipe being made.</summary>
    public int Recipe { get; set; }

    /// <summary>Item id of the product.</summary>
    public ushort Product { get; set; }

    /// <summary>How many batches the job makes.</summary>
    public byte Batches { get; set; }

    /// <summary>Total duration of the job in milliseconds.</summary>
    public int TotalMs { get; set; }

    /// <summary>Milliseconds already worked off before <see cref="StartedUtc"/>.</summary>
    public int ElapsedMs { get; set; }

    /// <summary>When the job last started running, or null while it is not running.</summary>
    public DateTime? StartedUtc { get; set; }

    /// <summary>Npc id of the pet helping with the job, or 0 for none.</summary>
    public int PetId { get; set; }

    /// <summary>Slot of the helping pet, or 0 for none.</summary>
    public byte PetSlot { get; set; }

    /// <summary>True when a magic sandglass was used for this job, which shortens its duration.</summary>
    public bool Sandglass { get; set; }

    /// <summary>Works out how far the job has come.</summary>
    /// <param name="now">The current time.</param>
    /// <returns>The milliseconds worked off so far, at most <see cref="TotalMs"/>.</returns>
    public int ElapsedNow(DateTime now) =>
        (int)Math.Min(TotalMs, ElapsedMs + (StartedUtc is { } s ? (now - s).TotalMilliseconds : 0));
}
