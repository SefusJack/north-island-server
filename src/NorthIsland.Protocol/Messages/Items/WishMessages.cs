using NorthIsland.Protocol.IO;
using NorthIsland.Protocol.Opcodes;

namespace NorthIsland.Protocol.Messages.Items;

/// <summary>Builders for the blessing and flower wish packets.</summary>
public static class WishMessages
{
    /// <summary>Tells the sender that a blessing was delivered (<see cref="Action23.BlessResult"/>, result 4).</summary>
    /// <param name="sender">Id of the player who sent it.</param>
    /// <param name="target">Id of the player who receives it.</param>
    /// <param name="text">The message text.</param>
    /// <returns>The packet.</returns>
    public static byte[] BlessDone(int sender, int target, string text) =>
        new PacketWriter(Action23.Code, Action23.BlessResult).U8(4).I32(sender).I32(target).RawString(text).ToArray();

    /// <summary>Refuses a blessing.</summary>
    /// <param name="code">The refusal code.</param>
    /// <returns>The packet.</returns>
    public static byte[] BlessRefused(byte code) => new PacketWriter(Action23.Code, Action23.BlessResult).U8(code).ToArray();

    /// <summary>Tells the sender that a flower was delivered (<see cref="Action23.FlowerResult"/>, result 1).</summary>
    /// <param name="sender">Id of the player who sent it.</param>
    /// <param name="target">Id of the player who receives it.</param>
    /// <param name="text">The message text.</param>
    /// <returns>The packet.</returns>
    public static byte[] FlowerToSender(int sender, int target, string text) =>
        new PacketWriter(Action23.Code, Action23.FlowerResult).U8(1).I32(sender).I32(target).RawString(text).ToArray();

    /// <summary>Tells the receiver of a flower (<see cref="Action23.FlowerResult"/>, result 2).</summary>
    /// <param name="sender">Id of the player who sent it.</param>
    /// <param name="target">Id of the player who receives it.</param>
    /// <param name="text">The message text.</param>
    /// <returns>The packet.</returns>
    public static byte[] FlowerToTarget(int sender, int target, string text) =>
        new PacketWriter(Action23.Code, Action23.FlowerResult).U8(2).I32(sender).I32(target).RawString(text).ToArray();

    /// <summary>Refuses a flower.</summary>
    /// <param name="code">The refusal code, sent after a 0.</param>
    /// <returns>The packet.</returns>
    public static byte[] FlowerRefused(byte code) => new PacketWriter(Action23.Code, Action23.FlowerResult).U8(0).U8(code).ToArray();
}
