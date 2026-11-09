namespace NorthIsland.GameData.Events;

/// <summary>Thrown when an event file or scene blob is malformed.</summary>
/// <param name="message">Description of the problem.</param>
public sealed class EveFormatException(
    string message) : Exception(message);
