namespace NorthIsland.Server.Data;

/// <summary>A pet asleep in a bed of the house.</summary>
public sealed class BedSleeper
{
    /// <summary>The bed of the furniture piece the pet sleeps in, counted from 1.</summary>
    public byte Bed { get; set; }

    /// <summary>The sleeper's number, unique among all the sleepers of the house; the client names the sleeper by it.</summary>
    public byte Id { get; set; }

    /// <summary>The sleeping pet, taken out of the character's pets while it sleeps.</summary>
    public Pet Pet { get; set; } = new();
}
