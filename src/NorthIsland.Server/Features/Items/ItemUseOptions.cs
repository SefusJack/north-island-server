namespace NorthIsland.Server.Features.Items;

/// <summary>Settings for the amounts and times of item effects.</summary>
public sealed class ItemUseOptions
{
    /// <summary>Multiplier on the experience that percentage items give, which is a percentage of the experience the current level needs.</summary>
    public double ExpPercentScale { get; set; } = 1.0;

    /// <summary>How many hours the protective experience items keep a player from losing experience.</summary>
    public double ProtectiveExpHours { get; set; } = 3;

    /// <summary>The emotion id the Yes-No Board shows for yes (the O).</summary>
    public byte YesNoEmotionO { get; set; } = 110;

    /// <summary>The emotion id the Yes-No Board shows for no (the X).</summary>
    public byte YesNoEmotionX { get; set; } = 111;

    /// <summary>The highest amity a pet can have.</summary>
    public int MaxAmity { get; set; } = 100;

    /// <summary>How many steps the plain Chaos Crystal takes off a mounted stone when it fails; 0 loses nothing.</summary>
    public int ChaosCrystalFailureLoss { get; set; }

    /// <summary>How many seconds a pet needs to carry items for the pet express.</summary>
    public int ExpressSeconds { get; set; } = 10;

    /// <summary>How many seconds a pet needs to look for one kind of item with a Dog Bone.</summary>
    public int ForageSeconds { get; set; } = 10;

    /// <summary>The most things of one kind a pet brings back from one Dog Bone trip.</summary>
    public int ForageMaxPerTrip { get; set; } = 10;

    /// <summary>The minutes of training island time a Training Ticket gives.</summary>
    public int TrainingTicketMinutes { get; set; } = 60;

    /// <summary>The most minutes of training island time a player can hold; 0 means no limit.</summary>
    public int TrainingMaxMinutes { get; set; }

    /// <summary>The chance of each potential step with a Potential Pill; the entry at index N is the step from potential N to N+1.</summary>
    public double[] PotentialChances { get; set; } = [1.0, 1.0, 1.0, 0.5, 0.25, 0.125, 0.0625, 0.03125, 0.015625, 0.0078125, 0.00390625, 0.001953125];

    /// <summary>The chance that the first Potential Pill adds to a potential step.</summary>
    public double PotentialPillBonus { get; set; } = 0.2;
}
