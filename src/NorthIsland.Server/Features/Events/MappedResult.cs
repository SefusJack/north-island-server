using NorthIsland.Protocol.Messages.Events;

namespace NorthIsland.Server.Features.Events;

/// <summary>A script result record turned into what the server does with it.</summary>
/// <param name="Action">What the server does.</param>
/// <param name="Wire">The record to send to the client, for actions that send one.</param>
/// <param name="Convey">
/// A second number the action uses: the convey index of a warp, the item of a point store row, join (1) or leave (2) of a team NPC,
/// the NPC to beat for a defined instance goal.
/// </param>
/// <param name="Description">A short text for the log.</param>
/// <param name="Service">
/// The main number the action uses, for example the service code, minigame kind, battle index, NPC id or amount of experience;
/// 0 when unused.
/// </param>
/// <param name="Packet">A ready packet to send, for the actions that send one.</param>
public readonly record struct MappedResult(
    ResultAction Action,
    EventResult Wire = default,
    int Convey = 0,
    string Description = "",
    int Service = 0,
    byte[]? Packet = null);
