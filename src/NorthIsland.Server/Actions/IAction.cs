namespace NorthIsland.Server.Actions;

/// <summary>One action of the protocol: the routes of the packets with one action number.</summary>
public interface IAction
{
    /// <summary>The action number.</summary>
    byte Code { get; }

    /// <summary>Adds the action's routes.</summary>
    /// <param name="routes">Where the routes of this action are added.</param>
    void Register(ActionRoutes routes);
}
