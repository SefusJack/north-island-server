using Microsoft.Extensions.Logging;
using NorthIsland.Protocol.Messages.Items;

namespace NorthIsland.Server.Features.PetCare;

/// <summary>Handles the Lost Property Office, which keeps nothing: its list is empty and every claim is refused.</summary>
/// <param name="logger">The logger.</param>
public sealed class LostPropertyHandlers(
    ILogger<LostPropertyHandlers> logger)
{
    /// <summary>Opens the office for a player with an empty list.</summary>
    /// <param name="session">The player's session.</param>
    public static void Open(GameSession session) => session.Send(LostPropertyMessages.List([]));

    /// <summary>Answers a claim for lost property with a refusal.</summary>
    /// <param name="session">The player's session.</param>
    /// <param name="body">The packet body, which is not read.</param>
    internal ValueTask OnReclaim(GameSession session, ReadOnlyMemory<byte> body)
    {
        if (session.State != SessionState.InWorld || session.ActiveCharacter is not { } c)
            return ValueTask.CompletedTask;
        logger.LogInformation("[{Session}] {Name} asks to redeem lost property: nothing is kept", session.Id, c.Name);
        session.Send(LostPropertyMessages.Refused(2));
        return ValueTask.CompletedTask;
    }
}
