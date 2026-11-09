namespace NorthIsland.Protocol.Opcodes;

/// <summary>Action id 85: instances (duplicate missions).</summary>
public static class Action85
{
    /// <summary>Action id of this packet group, 85.</summary>
    public const byte Code = 85;

    /// <summary>Client to server: opens the instance list (same value as <c>OrderList</c>).</summary>
    public const byte OpenList = 1;

    /// <summary>Client to server: asks for a page of the instance list (same value as <c>InitResult</c>).</summary>
    public const byte ListPage = 2;

    /// <summary>Client to server: creates an instance (same value as <c>JoinResult</c>).</summary>
    public const byte Create = 3;

    /// <summary>Client to server: asks for the details of an instance (same value as <c>DetailInfo</c>).</summary>
    public const byte Detail = 4;

    /// <summary>Client to server: joins an instance (same value as <c>State</c>).</summary>
    public const byte Join = 5;

    /// <summary>Client to server: leaves the instance (same value as <c>MemberList</c>).</summary>
    public const byte Leave = 6;

    /// <summary>Client to server: enters the instance.</summary>
    public const byte Enter = 9;

    /// <summary>Client to server: opens the members tab.</summary>
    public const byte MembersTab = 10;

    /// <summary>Client to server: asks for a page of members.</summary>
    public const byte MemberPage = 11;

    /// <summary>Client to server: asks for the instance progress.</summary>
    public const byte Progress = 12;

    /// <summary>Client to server: removes a member from the instance.</summary>
    public const byte Kick = 13;

    /// <summary>Client to server: the guild instance leave; the server accepts it and does nothing.</summary>
    public const byte LeaveGuild = 14;

    /// <summary>Server to client: a page of the instance list.</summary>
    public const byte OrderList = 1;

    /// <summary>Server to client: the result code of creating an instance.</summary>
    public const byte InitResult = 2;

    /// <summary>Server to client: the result code of joining an instance.</summary>
    public const byte JoinResult = 3;

    /// <summary>Server to client: the details of an instance with its members.</summary>
    public const byte DetailInfo = 4;

    /// <summary>Server to client: the player's instance state.</summary>
    public const byte State = 5;

    /// <summary>Server to client: a page of the instance members.</summary>
    public const byte MemberList = 6;

    /// <summary>Server to client: a member joined the instance.</summary>
    public const byte MemberJoined = 7;

    /// <summary>Server to client: one row of the instance list.</summary>
    public const byte OrderRow = 8;

    /// <summary>Server to client: the seconds left in the instance.</summary>
    public const byte RemainTime = 9;

    /// <summary>Server to client: the instance goal progress.</summary>
    public const byte Goal = 10;

    /// <summary>Server to client: an instance order was closed, with a reason.</summary>
    public const byte OrderClosed = 11;

    /// <summary>Server to client: a member left the instance.</summary>
    public const byte MemberLeft = 12;

    /// <summary>Sub code 13; no code uses it.</summary>
    public const byte StartNum = 13;
}
