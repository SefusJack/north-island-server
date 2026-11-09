namespace NorthIsland.Server.Features.Marriage;

/// <summary>The marriage steps that an event result can start.</summary>
public enum MarriageStep
{
    /// <summary>Apply to marry.</summary>
    Apply = 1,

    /// <summary>Hold the wedding ceremony.</summary>
    Wedding = 2,

    /// <summary>Apply for a divorce.</summary>
    Divorce = 3,
}
