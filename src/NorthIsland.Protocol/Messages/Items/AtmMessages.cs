using NorthIsland.Protocol.IO;
using NorthIsland.Protocol.Opcodes;

namespace NorthIsland.Protocol.Messages.Items;

/// <summary>Builders for the ATM packets (actions 23 and 26): the ATM balance and the point exchange.</summary>
public static class AtmMessages
{
    /// <summary>Highest balance an ATM account can hold.</summary>
    public const int MaxBalance = 400_000_000;

    /// <summary>Builds 23/165, which opens an ATM form on the client.</summary>
    /// <param name="form">Number of the form to open.</param>
    /// <returns>The packet.</returns>
    public static byte[] Form(byte form) => new PacketWriter(Action23.Code, Action23.AtmForm).U8(form).ToArray();

    /// <summary>Builds 26/8, which adds an amount to the ATM balance shown by the client.</summary>
    /// <param name="amount">Amount added.</param>
    /// <param name="fromId">Id of the player the amount came from, 0 when it came from nobody.</param>
    /// <returns>The packet.</returns>
    public static byte[] Added(int amount, int fromId = 0) => new PacketWriter(Action26.Code, Action26.AtmAdd).I32(amount).I32(fromId).ToArray();

    /// <summary>Builds 26/9, which subtracts an amount from the ATM balance shown by the client.</summary>
    /// <param name="amount">Amount taken.</param>
    /// <param name="toId">Id of the player the amount went to, 0 when it went to nobody.</param>
    /// <returns>The packet.</returns>
    public static byte[] Subtracted(int amount, int toId = 0) => new PacketWriter(Action26.Code, Action26.AtmSubtract).I32(amount).I32(toId).ToArray();

    /// <summary>Builds 26/10, which sets the ATM balance shown by the client.</summary>
    /// <param name="balance">The new balance.</param>
    /// <returns>The packet.</returns>
    public static byte[] Balance(int balance) => new PacketWriter(Action26.Code, Action26.AtmSet).I32(balance).ToArray();

    /// <summary>Builds 26/11, which tells the client the ATM account is full.</summary>
    /// <returns>The packet.</returns>
    public static byte[] Full() => new PacketWriter(Action26.Code, Action26.AtmFull).ToArray();

    /// <summary>Builds 23/200, which sets the exchange rate shown in the exchange form.</summary>
    /// <param name="goldPerPoint">Gold paid for one point.</param>
    /// <returns>The packet.</returns>
    public static byte[] ExchangeRate(int goldPerPoint) => new PacketWriter(Action23.Code, Action23.ExchangeRate).I32(goldPerPoint).ToArray();

    /// <summary>Builds 23/201, which reports the result of a gold exchange.</summary>
    /// <param name="goldNow">The player's gold after the exchange.</param>
    /// <param name="gained">Gold gained by the exchange.</param>
    /// <returns>The packet.</returns>
    public static byte[] ExchangeGold(int goldNow, int gained) => new PacketWriter(Action23.Code, Action23.ExchangeGold).I32(goldNow).I32(gained).ToArray();
}
