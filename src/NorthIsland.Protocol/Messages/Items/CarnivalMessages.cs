using NorthIsland.Protocol.IO;
using NorthIsland.Protocol.Opcodes;

namespace NorthIsland.Protocol.Messages.Items;

/// <summary>Builders for the results of the game-hall machines (action 71 and the 57 game packets).</summary>
public static class CarnivalMessages
{
    /// <summary>Builds the 71 packet (sub code is the machine number, then 2) that tells the client the player has no points.</summary>
    /// <param name="game">Machine number, used as the sub code.</param>
    /// <returns>The packet.</returns>
    public static byte[] NoPoints(byte game) => new PacketWriter(Action71.Code, game).U8(Action71.NoPoints).ToArray();

    /// <summary>Builds 57/3, which tells the client the function is unavailable.</summary>
    /// <returns>The packet.</returns>
    public static byte[] Unavailable() => new PacketWriter(Action57.Code, Action57.Unavailable).U8(1).ToArray();

    /// <summary>Builds 57/9/6, which starts a Lucky Cards play.</summary>
    /// <returns>The packet.</returns>
    public static byte[] PokeStart() => new PacketWriter(Action57.Code, Action57.Poke).U8(Action57.PokeStart).ToArray();

    /// <summary>Builds 57/9/3, which turns a Lucky Cards card.</summary>
    /// <param name="card">Number of the card.</param>
    /// <param name="face">Picture shown on the card.</param>
    /// <param name="turned">How many cards are turned now.</param>
    /// <returns>The packet.</returns>
    public static byte[] PokeFace(byte card, byte face, byte turned) =>
        new PacketWriter(Action57.Code, Action57.Poke).U8(Action57.PokeFlip).U8(card).U8(face).U8(turned).ToArray();

    /// <summary>Builds 57/12/1, the prizes of the wheel.</summary>
    /// <param name="codes">Prize codes of the 20 wheel slots.</param>
    /// <returns>The packet.</returns>
    public static byte[] WheelList(ReadOnlySpan<byte> codes) =>
        new PacketWriter(Action57.Code, Action57.Wheel).U8(Action57.WheelList).Bytes(codes).ToArray();

    /// <summary>Builds 57/12/2, the answer to a request to double the next spin.</summary>
    /// <param name="ok">True when the next spin is doubled.</param>
    /// <returns>The packet.</returns>
    public static byte[] WheelDoubled(bool ok) => new PacketWriter(Action57.Code, Action57.Wheel).U8(Action57.WheelDoubled).Bool(ok).ToArray();

    /// <summary>Builds 71/21, the result of a wheel spin.</summary>
    /// <param name="slot">Slot the wheel stops on, 1 to 20.</param>
    /// <param name="count">Number of items won.</param>
    /// <returns>The packet.</returns>
    public static byte[] WheelResult(byte slot, byte count) =>
        new PacketWriter(Action71.Code, 21).U8(Action71.Reward).U8(slot).U8(count).ToArray();

    /// <summary>Builds 57/9/4, which shows the picture of every Lucky Cards card.</summary>
    /// <param name="faces">The pictures of the 8 cards.</param>
    /// <returns>The packet.</returns>
    public static byte[] PokeReveal(ReadOnlySpan<byte> faces) =>
        new PacketWriter(Action57.Code, Action57.Poke).U8(Action57.PokeDone).Bytes(faces).ToArray();

    /// <summary>Builds 71/6, the result of an egg machine play.</summary>
    /// <param name="prize">Prize number, 1 to 10.</param>
    /// <param name="count">Number of items won.</param>
    /// <returns>The packet.</returns>
    public static byte[] Egg(byte prize, byte count) => new PacketWriter(Action71.Code, 6).U8(Action71.Won).U8(prize).U8(count).ToArray();

    /// <summary>Builds the result of a slot machine play (71/8 or 71/10).</summary>
    /// <param name="game">Machine number, used as the sub code.</param>
    /// <param name="prize">Prize number, 0 to 10.</param>
    /// <param name="reels">The three reel symbols.</param>
    /// <param name="count">Number of items won.</param>
    /// <returns>The packet.</returns>
    public static byte[] Spin(byte game, byte prize, byte[] reels, byte count) =>
        new PacketWriter(Action71.Code, game).U8(Action71.Won).U8(prize).U8(reels[0]).U8(reels[1]).U8(reels[2]).U8(count).ToArray();

    /// <summary>Builds the result of a jigsaw machine play (71/11, 71/12 or 71/14).</summary>
    /// <param name="game">Machine number, used as the sub code.</param>
    /// <param name="prize">Prize number, 0 to 10.</param>
    /// <param name="stops">The three stop symbols.</param>
    /// <param name="count">Number of items won.</param>
    /// <returns>The packet.</returns>
    public static byte[] Jigsaw(byte game, byte prize, byte[] stops, byte count) =>
        new PacketWriter(Action71.Code, game).U8(Action71.Reward).U8(prize).U8(stops[0]).U8(stops[1]).U8(stops[2]).U8(count).ToArray();

    /// <summary>Builds 71/16, the prize for a player who left Lucky Cards with some cards turned.</summary>
    /// <param name="itemId">Id of the prize item.</param>
    /// <returns>The packet.</returns>
    public static byte[] PokeLeavingPrize(ushort itemId) => new PacketWriter(Action71.Code, 16).U8(Action71.RewardAndExit).I32(itemId).ToArray();

    /// <summary>Builds the prize packet of a machine (71/7 or 71/9).</summary>
    /// <param name="game">Machine number, used as the sub code.</param>
    /// <param name="andExit">True to also close the machine.</param>
    /// <param name="itemId">Id of the prize item.</param>
    /// <param name="count">Number of items won.</param>
    /// <returns>The packet.</returns>
    public static byte[] Reward(byte game, bool andExit, ushort itemId, byte count) =>
        new PacketWriter(Action71.Code, game).U8(andExit ? Action71.RewardAndExit : Action71.Reward).I32(itemId).U8(count).ToArray();
}
