using NorthIsland.Protocol.IO;
using NorthIsland.Protocol.Opcodes;

namespace NorthIsland.Protocol.Messages.Items;

/// <summary>Builders for the training state packet (<see cref="Action23.TrainingState"/>).</summary>
public static class TrainingMessages
{
    /// <summary>Starts or updates the training timer.</summary>
    /// <param name="minutes">Minutes left.</param>
    /// <param name="sound">True to play the start sound.</param>
    /// <returns>The packet.</returns>
    public static byte[] State(ushort minutes, bool sound = false) =>
        new PacketWriter(Action23.Code, Action23.TrainingState).U8(1).U8(1).U16(minutes).U8(sound ? (byte)1 : (byte)0).ToArray();

    /// <summary>Ends the training.</summary>
    /// <returns>The packet.</returns>
    public static byte[] Over() => new PacketWriter(Action23.Code, Action23.TrainingState).U8(2).ToArray();

    /// <summary>Refuses a start because training already runs.</summary>
    /// <returns>The packet.</returns>
    public static byte[] AlreadyRunning() => new PacketWriter(Action23.Code, Action23.TrainingState).U8(3).ToArray();

    /// <summary>Refuses a start because the limit is reached.</summary>
    /// <returns>The packet.</returns>
    public static byte[] LimitReached() => new PacketWriter(Action23.Code, Action23.TrainingState).U8(4).ToArray();
}
