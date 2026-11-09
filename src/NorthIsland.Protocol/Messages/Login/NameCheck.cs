namespace NorthIsland.Protocol.Messages.Login;

/// <summary>Answer to a character name check, sent in <c>9/3</c>.</summary>
public enum NameCheck : byte
{
    /// <summary>Result 0: the name can be used.</summary>
    Ok = 0,

    /// <summary>Result 1: the name is taken or otherwise unavailable.</summary>
    Unavailable = 1,

    /// <summary>Result 2: the name is not valid.</summary>
    Invalid = 2,
}
