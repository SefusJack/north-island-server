namespace NorthIsland.Server.Features.Houses;

/// <summary>What a session is doing in a house: bathing or training.</summary>
public sealed class HouseLifeSessionState
{
    /// <summary>The furniture index of the tub the player bathes in; 0 when they do not bathe.</summary>
    public ushort BathFurn { get; set; }

    /// <summary>When the next bath step is due.</summary>
    public DateTime BathNext { get; set; }

    /// <summary>The running training, or null when the player does not train.</summary>
    public TrainingState? Training { get; set; }
}
