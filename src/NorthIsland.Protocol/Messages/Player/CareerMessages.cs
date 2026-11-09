using NorthIsland.Protocol.IO;
using NorthIsland.Protocol.Opcodes;

namespace NorthIsland.Protocol.Messages.Player;

/// <summary>Builders for the career choice packets.</summary>
public static class CareerMessages
{
    /// <summary>Shows the career choice form.</summary>
    /// <returns>The packet.</returns>
    public static byte[] ShowForm() => new PacketWriter(Action79.Code, Action79.Choose).ToArray();

    /// <summary>Answers a career choice.</summary>
    /// <param name="code">The result code.</param>
    /// <returns>The packet.</returns>
    public static byte[] Result(byte code) => new PacketWriter(Action79.Code, Action79.Result).U8(code).ToArray();

    /// <summary>Shows a player's job icon.</summary>
    /// <param name="playerId">Id of the player.</param>
    /// <param name="job">The job.</param>
    /// <returns>The packet.</returns>
    public static byte[] PlayerJob(int playerId, byte job) => new PacketWriter(Action5.Code, Action5.PlayerJob).I32(playerId).U8(job).ToArray();
}
