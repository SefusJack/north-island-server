namespace NorthIsland.Server.Data.Sql;

/// <summary>The database row of an account (table <c>accounts</c>).</summary>
public sealed class AccountRow
{
    /// <summary>Login id; the primary key.</summary>
    public int LoginId { get; set; }

    /// <summary>Account name.</summary>
    public string Name { get; set; } = "";

    /// <summary>Lower-cased account name; unique, for case-insensitive lookups.</summary>
    public string NameKey { get; set; } = "";

    /// <summary>When the account was created.</summary>
    public DateTime CreatedUtc { get; set; }

    /// <summary>When the account last logged in, or null if it never has.</summary>
    public DateTime? LastLoginUtc { get; set; }

    /// <summary>Seconds the account has been online in total.</summary>
    public long OnlineSeconds { get; set; }

    /// <summary>The account's mall points.</summary>
    public int MallPoints { get; set; }

    /// <summary>When the row was last written.</summary>
    public DateTime SavedUtc { get; set; }

    /// <summary>
    /// The whole account as JSON (column type <c>jsonb</c>); the other columns repeat parts of it for queries.
    /// </summary>
    public string Data { get; set; } = "";

    /// <summary>The rows of the account's characters.</summary>
    public List<CharacterRow> Characters { get; set; } = [];
}
