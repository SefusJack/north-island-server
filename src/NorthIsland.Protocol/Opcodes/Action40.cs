namespace NorthIsland.Protocol.Opcodes;

/// <summary>Sub codes of action <c>40</c>: the hotkey bar.</summary>
public static class Action40
{
    /// <summary>The action byte <c>40</c> that starts every packet of this group.</summary>
    public const byte Code = 40;

    /// <summary>Both ways: one hotkey slot; the server sends them all to the client.</summary>
    public const byte Slot = 1;
}
