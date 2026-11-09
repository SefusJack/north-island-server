using System.Text.Json;

namespace NorthIsland.Server.Data;

/// <summary>Stores each document as an indented JSON file in a directory.</summary>
/// <param name="directory">The directory that holds the files.</param>
public sealed class FileDocumentStore(
    string directory) : IDocumentStore
{
    /// <summary>The serializer options: indented output.</summary>
    private static readonly JsonSerializerOptions Json = new() { WriteIndented = true };

    /// <summary>Gives the path of a document's file.</summary>
    /// <param name="name">The document name.</param>
    /// <returns>The path of <c>name.json</c> in the store's directory.</returns>
    public string PathOf(string name) => Path.Combine(directory, name + ".json");

    public T? Load<T>(string name) where T : class
    {
        var path = PathOf(name);
        return File.Exists(path) ? JsonSerializer.Deserialize<T>(File.ReadAllText(path)) : null;
    }

    public void Save<T>(string name, T document)
    {
        var path = PathOf(name);
        Directory.CreateDirectory(directory);
        var temp = path + ".tmp";
        File.WriteAllText(temp, JsonSerializer.Serialize(document, Json));
        File.Move(temp, path, overwrite: true);
    }
}
