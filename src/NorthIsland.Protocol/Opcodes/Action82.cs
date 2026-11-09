namespace NorthIsland.Protocol.Opcodes;

/// <summary>Action id 82: the notice boards, public and guild.</summary>
public static class Action82
{
    /// <summary>Action id of this packet group, 82.</summary>
    public const byte Code = 82;

    /// <summary>Client to server: posts a notice.</summary>
    public const byte Submit = 1;

    /// <summary>Client to server: asks for a notice page.</summary>
    public const byte List = 2;

    /// <summary>Client to server: opens a notice.</summary>
    public const byte Open = 3;

    /// <summary>Client to server: deletes a notice.</summary>
    public const byte Delete = 4;

    /// <summary>Client to server: asks whether the player may post.</summary>
    public const byte AskAdd = 5;

    /// <summary>Client to server: replies to a notice.</summary>
    public const byte Reply = 6;

    /// <summary>Client to server: posts a guild notice.</summary>
    public const byte GuildSubmit = 7;

    /// <summary>Client to server: asks for a guild notice page.</summary>
    public const byte GuildList = 8;

    /// <summary>Client to server: opens a guild notice.</summary>
    public const byte GuildOpen = 9;

    /// <summary>Client to server: deletes a guild notice.</summary>
    public const byte GuildDelete = 10;

    /// <summary>Client to server: asks whether the player may post a guild notice.</summary>
    public const byte GuildAskAdd = 11;

    /// <summary>Server to client: the rows of a notice page.</summary>
    public const byte Rows = 1;

    /// <summary>Server to client: the content of a notice.</summary>
    public const byte Content = 2;

    /// <summary>Server to client: the notice count and page number.</summary>
    public const byte PageInfo = 4;

    /// <summary>Server to client: the board is full.</summary>
    public const byte Full = 6;

    /// <summary>Server to client: the notice was not found.</summary>
    public const byte NotFound = 7;

    /// <summary>Server to client: shows the notice posting form (public or guild).</summary>
    public const byte ShowSend = 8;

    /// <summary>Server to client: shows the notice board.</summary>
    public const byte Show = 9;

    /// <summary>Server to client: the rows of a guild notice page.</summary>
    public const byte GuildRows = 11;

    /// <summary>Server to client: the content of a guild notice.</summary>
    public const byte GuildContent = 12;

    /// <summary>Server to client: the guild notice count and page number.</summary>
    public const byte GuildPageInfo = 14;

    /// <summary>Server to client: a guild notice was deleted.</summary>
    public const byte GuildDeleted = 15;

    /// <summary>Server to client: the player has no right for the action.</summary>
    public const byte NoRight = 16;
}
