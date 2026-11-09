namespace NorthIsland.Server.Features.Marriage;

/// <summary>Settings of the marriage feature.</summary>
public sealed class MarriageOptions
{
    /// <summary>Milliseconds between the steps of the wedding ceremony.</summary>
    public int WeddingStepMs { get; set; } = 3000;

    /// <summary>Whether the wedding requires both partners to wear the wedding clothes and ring.</summary>
    public bool WeddingClothesCheck { get; set; } = true;
}
