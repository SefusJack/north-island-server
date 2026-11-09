namespace NorthIsland.Protocol.Opcodes;

/// <summary>Sub codes of action <c>33</c>: player option switches.</summary>
public static class Action33
{
    /// <summary>The action byte <c>33</c> that starts every packet of this group.</summary>
    public const byte Code = 33;

    /// <summary>Both ways: option 1; the client sets it, and the server also uses it to send option codes to the client.</summary>
    public const byte Option1 = 1;

    /// <summary>Client to server: sets option 2.</summary>
    public const byte Option2 = 2;

    /// <summary>Client to server: the chat channels that are enabled, one bit per channel.</summary>
    public const byte ChannelBits = 3;

    /// <summary>Client to server: sets option 6.</summary>
    public const byte Option6 = 4;

    /// <summary>Client to server: the state of the party option.</summary>
    public const byte PartyOption = 5;

    /// <summary>Server to client: the states of options 1, 2 and 6.</summary>
    public const byte OptionState = 2;

    /// <summary>Value sent in an option 1 packet to tell the client the party option is off.</summary>
    public const byte PartyOptionOff = 10;
}
