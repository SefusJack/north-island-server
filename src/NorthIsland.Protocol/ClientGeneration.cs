namespace NorthIsland.Protocol;

/// <summary>The generation of client a session talks to, which decides the packet layouts that differ between them.</summary>
public enum ClientGeneration
{
    /// <summary>The 2010 client, login version 1062.</summary>
    Client2010,

    /// <summary>The 2014 client, login version 1102 and later.</summary>
    Client2014,
}
