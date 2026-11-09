using System.Text.Json.Serialization;

namespace NorthIsland.Server.Data;

/// <summary>A player account as stored in its account file, with its characters.</summary>
public sealed class Account
{
    /// <summary>The numeric id of the account; the id the characters of the account use as their player id.</summary>
    public int LoginId { get; set; }

    /// <summary>The login name.</summary>
    public string Name { get; set; } = "";

    /// <summary>The hash of the account password, checked with the password hasher.</summary>
    public string PasswordHash { get; set; } = "";

    /// <summary>When the account was created (UTC).</summary>
    public DateTime CreatedUtc { get; set; }

    /// <summary>When the account last logged in (UTC); null before the first login.</summary>
    public DateTime? LastLoginUtc { get; set; }

    /// <summary>The seconds the account has spent online in finished sessions.</summary>
    public long OnlineSeconds { get; set; }

    /// <summary>True for a GM account; not written to the account file when false.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool Gm { get; set; }

    /// <summary>The account's characters.</summary>
    public List<Character> Characters { get; set; } = [];

    /// <summary>The account's item mall points.</summary>
    public int MallPoints { get; set; }

    /// <summary>True once the starting mall points were given to the account.</summary>
    public bool MallPointsGiven { get; set; }

    /// <summary>The bonus points the account has earned in the item mall.</summary>
    public int BonusPoints { get; set; }

    /// <summary>Finds the character in a character slot.</summary>
    /// <param name="slot">The character slot.</param>
    /// <returns>The character, or null when the slot is empty.</returns>
    public Character? CharacterInSlot(byte slot) => Characters.Find(c => c.Slot == slot);
}
