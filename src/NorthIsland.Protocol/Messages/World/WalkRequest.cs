using NorthIsland.Protocol.IO;

namespace NorthIsland.Protocol.Messages.World;

/// <summary>A walk or stop request from the client (<c>6/1</c> and <c>6/2</c>).</summary>
/// <param name="Direction">Direction byte.</param>
/// <param name="X">Destination x.</param>
/// <param name="Y">Destination y.</param>
/// <param name="Check">Check byte; see <see cref="ExpectedCheck"/>.</param>
/// <param name="Noise">Noise byte.</param>
public sealed record WalkRequest(
    byte Direction,
    ushort X,
    ushort Y,
    byte Check,
    byte Noise)
{
    /// <summary>Reads a walk request from a packet body.</summary>
    /// <param name="body">The packet body after the action and sub code.</param>
    /// <returns>The request.</returns>
    public static WalkRequest Parse(ReadOnlySpan<byte> body)
    {
        var r = new PacketReader(body);
        return new WalkRequest(r.U8(), r.U16(), r.U16(), r.U8(), r.U8());
    }

    /// <summary>Computes the check byte a client sends for a player id and level.</summary>
    /// <param name="playerId">Id of the player.</param>
    /// <param name="level">Level of the player.</param>
    /// <returns>The expected value of <see cref="Check"/>.</returns>
    public static byte ExpectedCheck(int playerId, byte level) => (byte)(playerId % 13 + level);
}
