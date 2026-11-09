namespace NorthIsland.Server.Data;

/// <summary>Storage of named server-wide documents (state that belongs to no single account).</summary>
public interface IDocumentStore
{
    /// <summary>Reads a document.</summary>
    /// <typeparam name="T">The document type.</typeparam>
    /// <param name="name">The document name.</param>
    /// <returns>The document, or null when none is stored under that name.</returns>
    T? Load<T>(string name) where T : class;

    /// <summary>Stores a document, replacing any earlier one of the same name.</summary>
    /// <typeparam name="T">The document type.</typeparam>
    /// <param name="name">The document name.</param>
    /// <param name="document">The document to store.</param>
    void Save<T>(string name, T document);
}
