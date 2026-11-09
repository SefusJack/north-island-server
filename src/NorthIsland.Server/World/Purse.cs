using NorthIsland.Protocol.Messages.Player;

namespace NorthIsland.Server.World;

/// <summary>Gold handling for characters.</summary>
public static class Purse
{
    /// <summary>
    /// Adds gold to a character up to the purse limit, telling the client the amount received and, when some did not fit,
    /// that the money bag is full.
    /// </summary>
    /// <param name="session">The player's session.</param>
    /// <param name="character">The character.</param>
    /// <param name="gold">The gold to add.</param>
    /// <returns>The gold actually added.</returns>
    public static int Credit(GameSession session, Character character, long gold)
    {
        var newGold = (int)Math.Min(BankHandlers.MaxGold, character.Gold + gold);
        var added = newGold - character.Gold;
        if (added > 0)
            session.Send(PlayerMessages.MoneyChange(added));
        character.Gold = newGold;
        if (added < gold)
            session.Send(PlayerMessages.MoneyBagFull());
        return added;
    }
}
