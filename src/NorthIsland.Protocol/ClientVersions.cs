using NorthIsland.Protocol.IO;

namespace NorthIsland.Protocol;

/// <summary>The login versions of the known clients.</summary>
public static class ClientVersions
{
    /// <summary>The login version of the 2010 client.</summary>
    public const ushort Version2010 = 1062;

    /// <summary>The login version of the 2014 client.</summary>
    public const ushort Version2014 = 1102;

    /// <summary>The client generation of a login version.</summary>
    /// <param name="version">The login version the client sent.</param>
    /// <returns>The 2014 generation from <see cref="Version2014"/> up, otherwise the 2010 one.</returns>
    public static ClientGeneration For(ushort version) => version >= Version2014 ? ClientGeneration.Client2014 : ClientGeneration.Client2010;

    /// <summary>Writes an HP or max-HP field in the form of the client generation.</summary>
    /// <param name="w">The writer.</param>
    /// <param name="value">The HP amount.</param>
    /// <param name="client">The client generation.</param>
    /// <returns>The writer: an i32 for the 2014 client, a u16 clamped to 0..65535 for the 2010 one.</returns>
    public static PacketWriter Hp(this PacketWriter w, int value, ClientGeneration client) =>
        client == ClientGeneration.Client2014 ? w.I32(value) : w.U16((ushort)Math.Clamp(value, 0, ushort.MaxValue));
}
