using System.Collections.Frozen;

namespace NorthIsland.Protocol;

/// <summary>Tells which actions have a sub code byte after the action number.</summary>
public static class SubCodes
{
    /// <summary>The actions the client sends without a sub code byte.</summary>
    private static readonly FrozenSet<byte> ClientNoSub = [0, 1, 3, 7, 42, 54, 55, 58, 59, 61, 77];

    /// <summary>The actions the server sends without a sub code byte.</summary>
    private static readonly FrozenSet<byte> ServerNoSub = [3, 4, 7, 12];

    /// <summary>Tells whether packets of an action start with an action byte followed by a sub code byte.</summary>
    /// <param name="action">The action number.</param>
    /// <param name="fromClient">True for a packet the client sends, false for one the server sends.</param>
    /// <returns>False when the action is one of those sent without a sub code in that direction.</returns>
    public static bool HasSub(byte action, bool fromClient) => !(fromClient ? ClientNoSub : ServerNoSub).Contains(action);
}
