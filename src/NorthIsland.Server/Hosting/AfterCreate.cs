namespace NorthIsland.Server.Hosting;

/// <summary>What happens after a new character is created.</summary>
public enum AfterCreate
{
    /// <summary>The new character enters the world at once.</summary>
    EnterWorld,

    /// <summary>The client is sent back to the character list.</summary>
    CharacterList,
}
