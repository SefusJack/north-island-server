using NorthIsland.Protocol.Opcodes;

namespace NorthIsland.Server.Actions;

/// <summary>Registers the routes of the packets the client sends with action 15: pets, riding, vehicles and the constellation.</summary>
/// <param name="pet">The <c>PetHandlers</c> that serve the packets of this action.</param>
/// <param name="social">The <c>SocialHandlers</c> that serve the packets of this action.</param>
/// <param name="vehicle">The <c>VehicleHandlers</c> that serve the packets of this action.</param>
public sealed class Action15Pets(
    PetHandlers pet,
    SocialHandlers social,
    VehicleHandlers vehicle) : IAction
{
    public byte Code => Action15.Code;

    public void Register(ActionRoutes routes) => routes
        .On(Action15.ExilePet, "exile pet", pet.OnExile) // 15/2: send a pet away for good
        .On(Action15.RenamePet, "rename pet", pet.OnRename) // 15/6: rename a pet
        .On(Action15.Board, "board vehicle", vehicle.OnBoard) // 15/7: board the vehicle that is placed
        .On(Action15.Park, "leave vehicle", vehicle.OnPark) // 15/8: get off the vehicle
        .On(Action15.PlaceFromBag, "place vehicle", (s, b) => vehicle.OnPlace(s, b, board: false)) // 15/9: place a vehicle from the bag on the map
        .On(Action15.ClickVehicle, "click vehicle", vehicle.OnClick) // 15/10: click on the vehicle: get off or take it back
        .On(Action15.RidePet, "ride pet", pet.OnRide) // 15/11: ride a pet
        .On(Action15.LeavePet, "leave pet", pet.OnLeaveRide) // 15/12: get off the pet
        .On(Action15.WreckSeen, "vehicle wreck seen", vehicle.OnWreckSeen) // 15/13: the client has seen the vehicle wreck
        .On(Action15.WalkIntoWater, "walk into water", (s, b) => vehicle.OnPlace(s, b, board: true)) // 15/14: walk into water: place the vehicle and board it
        .On(Action15.Constellation, "constellation", social.OnConstellation); // 15/15: set the two constellations
}
