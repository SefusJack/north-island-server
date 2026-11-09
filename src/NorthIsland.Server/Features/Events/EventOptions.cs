namespace NorthIsland.Server.Features.Events;

/// <summary>Settings of the event scripts.</summary>
public sealed class EventOptions
{
    /// <summary>Settings of the PvP hall service.</summary>
    public PvpHallOptions PvpHall { get; set; } = new();

    /// <summary>Settings of the New Year event.</summary>
    public NewYearOptions NewYear { get; set; } = new();

    /// <summary>The state sent for scene objects at map entry; a value outside 0 to 255 sends none.</summary>
    public int NpcSpawnState { get; set; }

    /// <summary>How many seconds a pickup object stays empty after it was emptied; 0 or less never empties it.</summary>
    public int PickupRefillSeconds { get; set; } = 300;

    /// <summary>The item ids a team NPC wears when it joins, by NPC id.</summary>
    public Dictionary<int, List<ushort>> TeamNpcEquipment { get; set; } = new()
    {
        [14162] = [10063], // 14162 Roca, 10063 Machete
        [14156] = [25028], // 14156 Xaolan, 25028 Xaolan's Jade
    };

    /// <summary>How many hours a hired team NPC stays; 0 or less means no limit.</summary>
    public double HireLeaseHours { get; set; } = 3;
}
