using Microsoft.Extensions.Options;
using NorthIsland.Protocol.Messages.Player;

namespace NorthIsland.Server.Features.Storage;

/// <summary>
/// The rules of the Space Contract: a timed bonus whose parts (stock keeper, hotel, tent, garage) enlarge or unlock storage, kept per character.
/// </summary>
/// <param name="options">The storage options that hold the rules.</param>
public sealed class SpaceContract(
    IOptions<StorageOptions> options)
{
    /// <summary>The flags of the contract's parts: stock keeper (Item Bag), hotel, tent and garage.</summary>
    public const byte Stock = 1, Hotel = 2, Tent = 4, Garage = 8;

    /// <summary>The item ids of the Space Contract (all four parts) and of the Space Agreement (tent and garage).</summary>
    public const ushort Contract = 34133, Agreement = 34148; // 34133 Space Contract, 34148 Space Agreement

    /// <summary>The Space Contract rules from the storage options.</summary>
    private SpaceContractOptions Options => options.Value.SpaceContract;

    /// <summary>Returns the parts a contract item grants.</summary>
    /// <param name="itemId">The item id.</param>
    /// <returns>The part flags, 0 when the item is not a contract item.</returns>
    public static byte FlagsOf(ushort itemId) => itemId switch
    {
        Contract => Stock | Hotel | Tent | Garage, // Space Contract: all four parts
        Agreement => Tent | Garage, // Space Agreement: tent and garage
        _ => 0 // not a contract item
    };

    /// <summary>Tells whether the character has a contract that has not run out.</summary>
    /// <param name="c">The character.</param>
    /// <param name="now">The current time, UTC.</param>
    /// <returns>True when the end time lies ahead and some part is set.</returns>
    public static bool Active(Character c, DateTime now) => c.SpaceUntilUtc is { } until && until > now && c.SpaceFlags != 0;

    /// <summary>
    /// Tells whether the character may use a part. The stock keeper and garage parts always need a running contract with that part; the others only
    /// when the options require it.
    /// </summary>
    /// <param name="c">The character.</param>
    /// <param name="part">One of the part flags.</param>
    /// <returns>True when the part is usable.</returns>
    public bool Has(Character c, byte part) =>
        !(Options.Required || part == Stock || part == Garage) || Active(c, GameClock.UtcNow) && (c.SpaceFlags & part) != 0;

    /// <summary>Counts the hours the contract still runs, rounded up.</summary>
    /// <param name="c">The character.</param>
    /// <param name="now">The current time, UTC.</param>
    /// <returns>The hours left, 0 when no contract runs.</returns>
    public static int HoursLeft(Character c, DateTime now) =>
        Active(c, now) ? (int)Math.Ceiling((c.SpaceUntilUtc!.Value - now).TotalHours) : 0;

    /// <summary>Gives the size of the character's house room: the configured tent size (1 to 3) while a tent part runs, else 1.</summary>
    /// <param name="c">The character.</param>
    /// <returns>The room size.</returns>
    public byte TentSize(Character c) => Active(c, GameClock.UtcNow) && (c.SpaceFlags & Tent) != 0 ? Math.Clamp(Options.TentSize, (byte)1, (byte)3) : (byte)1;

    /// <summary>
    /// Counts the pet storage slots of the hotel the character may use: all of them with a hotel part, else the free slots of the options.
    /// </summary>
    /// <param name="c">The character.</param>
    /// <returns>The number of slots.</returns>
    public int HotelSlots(Character c) => Has(c, Hotel) ? InnMessages.MaxStoredPets : Math.Min(InnMessages.MaxStoredPets, Options.HotelFreeSlots);

    /// <summary>Gives the first row of the house room that lies inside the room at a size; furniture placed on a smaller row is outside it.</summary>
    /// <param name="size">The room size, 1 to 3.</param>
    /// <returns>The row.</returns>
    public static int TopRow(byte size) => size switch
    {
        3 => 27, // size 3: first row 27
        2 => 32, // size 2: first row 32
        _ => 38 // any other size: first row 38
    };

    /// <summary>
    /// Uses a contract item: adds the days per use to the running time (from its end when it still runs) and joins the item's parts to the flags.
    /// Refused for another item or when the total would pass the maximum.
    /// </summary>
    /// <param name="c">The character.</param>
    /// <param name="itemId">The contract item.</param>
    /// <param name="now">The current time, UTC.</param>
    /// <returns>True when the contract was extended.</returns>
    public bool Use(Character c, ushort itemId, DateTime now)
    {
        var flags = FlagsOf(itemId);
        var from = Active(c, now) ? c.SpaceUntilUtc!.Value : now;
        var until = from.AddDays(Options.DaysPerUse);
        if (flags == 0 || (until - now).TotalDays > Options.MaxDays)
            return false;
        c.SpaceFlags = (byte)((Active(c, now) ? c.SpaceFlags : 0) | flags);
        c.SpaceUntilUtc = until;
        return true;
    }

    /// <summary>Clears the contract once it has run out.</summary>
    /// <param name="c">The character.</param>
    /// <param name="now">The current time, UTC.</param>
    /// <returns>True when it had run out and was cleared by this call, so the caller must tell the client.</returns>
    public static bool Ended(Character c, DateTime now)
    {
        if (c.SpaceUntilUtc is null || Active(c, now))
            return false;
        c.SpaceUntilUtc = null;
        c.SpaceFlags = 0;
        return true;
    }
}
