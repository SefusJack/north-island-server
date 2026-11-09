using NorthIsland.Protocol.IO;
using NorthIsland.Protocol.Opcodes;

namespace NorthIsland.Protocol.Messages.Player;

/// <summary>Builders for the packets of the reborn wish.</summary>
public static class RebornWishMessages
{
    /// <summary>Lists the pets that can be wished for (<c>66/11</c>).</summary>
    /// <param name="growKeys">Grow key of each pet.</param>
    /// <returns>The packet.</returns>
    public static byte[] List(IReadOnlyCollection<byte> growKeys)
    {
        var w = new PacketWriter(Action66.Code, Action66.WishList).U8((byte)growKeys.Count);
        foreach (var key in growKeys)
            w.U8(key).U8(0).U8(0).I32(0).I32(0);
        return w.ToArray();
    }

    /// <summary>Opens the choice of whom to wish for (<c>66/15</c>).</summary>
    /// <returns>The packet.</returns>
    public static byte[] Choice() => new PacketWriter(Action66.Code, Action66.Choice).ToArray();

    /// <summary>Removes a pet from the wish list (<c>66/13</c>).</summary>
    /// <param name="npcId">Npc id of the pet.</param>
    /// <returns>The packet.</returns>
    public static byte[] Removed(ushort npcId) => new PacketWriter(Action66.Code, Action66.Removed).U16(npcId).ToArray();

    /// <summary>Sends the greeting of the wish.</summary>
    /// <returns>The packet.</returns>
    public static byte[] Greeting() => new PacketWriter(Action66.Code, Action66.Greeting).ToArray();
}
