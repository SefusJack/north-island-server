namespace NorthIsland.Server.Data.Sql;

/// <summary>The database row of a named document (table <c>documents</c>).</summary>
public sealed class DocumentRow
{
    /// <summary>Document name; the primary key.</summary>
    public string Name { get; set; } = "";

    /// <summary>The document as JSON (column type <c>jsonb</c>).</summary>
    public string Data { get; set; } = "";

    /// <summary>When the document was last written.</summary>
    public DateTime SavedUtc { get; set; }
}
