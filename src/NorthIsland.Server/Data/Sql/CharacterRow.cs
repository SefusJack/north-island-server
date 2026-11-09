namespace NorthIsland.Server.Data.Sql;

/// <summary>
/// The database row of a character (table <c>characters</c>), repeating a few fields of the account's JSON for queries.
/// </summary>
public sealed class CharacterRow
{
    /// <summary>Login id of the owning account; part of the key.</summary>
    public int LoginId { get; set; }

    /// <summary>Character slot in the account; part of the key.</summary>
    public short Slot { get; set; }

    /// <summary>Character name.</summary>
    public string Name { get; set; } = "";

    /// <summary>Lower-cased character name, indexed.</summary>
    public string NameKey { get; set; } = "";

    /// <summary>Character level.</summary>
    public short Level { get; set; }

    /// <summary>Id of the map the character is on.</summary>
    public int MapId { get; set; }
}
