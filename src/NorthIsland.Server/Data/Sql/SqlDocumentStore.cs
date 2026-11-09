using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace NorthIsland.Server.Data.Sql;

/// <summary>
/// Document store backed by the <c>documents</c> table; a document that is not there is imported from the JSON files, if a folder is given.
/// </summary>
/// <param name="options">The context options, naming the database.</param>
/// <param name="importDirectory">A folder of document files to import missing documents from, or null.</param>
/// <param name="logger">Logs imports.</param>
public sealed class SqlDocumentStore(
    DbContextOptions<ServerDbContext> options,
    string? importDirectory,
    ILogger<SqlDocumentStore> logger) : IDocumentStore
{
    /// <summary>Options for the document JSON.</summary>
    private static readonly JsonSerializerOptions Json = new();

    public T? Load<T>(string name) where T : class
    {
        using (var db = new ServerDbContext(options))
        {
            var data = db.Documents.AsNoTracking().Where(d => d.Name == name).Select(d => d.Data).SingleOrDefault();
            if (data is not null)
                return JsonSerializer.Deserialize<T>(data, Json);
        }
        if (importDirectory is null)
            return null;
        var files = new FileDocumentStore(importDirectory);
        var document = files.Load<T>(name);
        if (document is not null)
        {
            Save(name, document);
            logger.LogInformation("Imported {Path} into the database", files.PathOf(name));
        }
        return document;
    }

    public void Save<T>(string name, T document)
    {
        try
        {
            using var db = new ServerDbContext(options);
            var row = db.Documents.SingleOrDefault(d => d.Name == name);
            if (row is null)
            {
                row = new DocumentRow { Name = name };
                db.Documents.Add(row);
            }
            row.Data = JsonSerializer.Serialize(document, Json);
            row.SavedUtc = GameClock.UtcNow;
            db.SaveChanges();
        }
        catch (Exception ex) when (ex is DbUpdateException or System.Data.Common.DbException or InvalidOperationException)
        {
            throw new IOException($"Saving document '{name}' to the database failed: {ex.Message}", ex);
        }
    }
}
