namespace NorthIsland.Server.Features.Events;

/// <summary>The table of hire tiers of team NPCs.</summary>
public static class HireTiers
{

    /// <summary>Looks up a hire tier.</summary>
    /// <param name="value">The tier number.</param>
    /// <returns>The tier; null when the number is unknown.</returns>
    public static HireTier? Find(int value) => value switch
    {
        10 => new HireTier(26, 80, 32947), // 26 Points, 80 MinLevel, 32947 It costs 26 points. (Required level 80)
        15 => new HireTier(15, 0, 0), // 15 Points
        20 => new HireTier(45, 100, 32948), // 45 Points, 100 MinLevel, 32948 It costs 45 points. (Required level 100)
        _ => null, // any other number: no tier
    };

    /// <summary>Works out when a lease that starts now ends.</summary>
    /// <param name="now">The time the lease starts.</param>
    /// <param name="hours">The length of the lease in hours.</param>
    /// <returns>The end; null when <paramref name="hours"/> is 0 or less, meaning no limit.</returns>
    public static DateTime? LeaseEnd(DateTime now, double hours) => hours > 0 ? now.AddHours(hours) : null;

    /// <summary>Finds the pets of a character, in the team or in storage, whose lease has run out.</summary>
    /// <param name="character">The character.</param>
    /// <param name="now">The time to test against.</param>
    /// <returns>The pets whose lease ended at or before <paramref name="now"/>.</returns>
    public static List<Pet> Expired(Character character, DateTime now) =>
        [.. character.Pets.Concat(character.StoredPets).Where(p => p.LeaseUntil is { } until && until <= now)];
}
