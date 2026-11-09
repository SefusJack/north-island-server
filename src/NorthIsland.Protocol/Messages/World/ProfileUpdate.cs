using NorthIsland.Protocol.IO;

namespace NorthIsland.Protocol.Messages.World;

/// <summary>The profile the client sends in <c>10/2</c>.</summary>
/// <param name="HeartNumber">Heart number.</param>
/// <param name="BloodType">Blood type byte.</param>
/// <param name="BirthYear">Birth year byte.</param>
/// <param name="BirthMonth">Birth month.</param>
/// <param name="BirthDay">Birth day.</param>
public sealed record ProfileUpdate(
    byte HeartNumber,
    byte BloodType,
    byte BirthYear,
    byte BirthMonth,
    byte BirthDay)
{
    /// <summary>Reads a profile from a packet body.</summary>
    /// <param name="body">The packet body after the action and sub code.</param>
    /// <returns>The profile.</returns>
    public static ProfileUpdate Parse(ReadOnlySpan<byte> body)
    {
        var r = new PacketReader(body);
        return new ProfileUpdate(r.U8(), r.U8(), r.U8(), r.U8(), r.U8());
    }
}
