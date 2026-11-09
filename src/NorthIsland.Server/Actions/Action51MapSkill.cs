using NorthIsland.Protocol.Opcodes;

namespace NorthIsland.Server.Actions;

/// <summary>Registers the routes of the packets the client sends with action 51: casting a skill on the map.</summary>
/// <param name="mapUse">The <c>MapUseHandlers</c> that serve the packets of this action.</param>
public sealed class Action51MapSkill(
    MapUseHandlers mapUse) : IAction
{
    public byte Code => Action51.Code;

    public void Register(ActionRoutes routes) => routes
        .On(Action51.CastOnMap, "cast skill on the map", mapUse.OnCast); // 51/1: cast a skill on the map
}
