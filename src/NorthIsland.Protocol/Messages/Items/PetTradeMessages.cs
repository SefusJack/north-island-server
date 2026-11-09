using NorthIsland.Protocol.IO;
using NorthIsland.Protocol.Messages.Player;
using NorthIsland.Protocol.Opcodes;

namespace NorthIsland.Protocol.Messages.Items;

/// <summary>Builders for the pet trade packets (action 25).</summary>
public static class PetTradeMessages
{
    /// <summary>Size in bytes of the pet info block.</summary>
    public const int InfoBlockSize = 84;

    /// <summary>Size in bytes of the pet info block for the 2014 client.</summary>
    public const int InfoBlockSize2014 = 86;

    /// <summary>Gives the size of the pet info block for a client generation.</summary>
    /// <param name="client">Client generation.</param>
    /// <returns>The size in bytes.</returns>
    public static int InfoBlockSizeFor(ClientGeneration client) => client == ClientGeneration.Client2014 ? InfoBlockSize2014 : InfoBlockSize;

    /// <summary>Builds the packet that asks a player to trade pets.</summary>
    /// <param name="partnerId">Id of the trade partner.</param>
    /// <returns>The packet.</returns>
    public static byte[] Start(int partnerId) => new PacketWriter(Action25.Code, Action25.PetRequest).I32(partnerId).ToArray();

    /// <summary>Builds the packet that reports a trade result.</summary>
    /// <param name="result">The result.</param>
    /// <returns>The packet.</returns>
    public static byte[] Result(PetTradeResult result) => new PacketWriter(Action25.Code, Action25.PetOffer).U8((byte)result).ToArray();

    /// <summary>Builds 25/12, the pet and gold the partner offers.</summary>
    /// <param name="gold">Gold offered.</param>
    /// <param name="pet">The offered pet, null for none.</param>
    /// <param name="maxHp">Maximum HP of the pet.</param>
    /// <param name="maxSp">Maximum SP of the pet.</param>
    /// <param name="client">Client generation.</param>
    /// <returns>The packet.</returns>
    public static byte[] PartnerOffer(
        int gold,
        PetInfo? pet,
        int maxHp,
        int maxSp,
        ClientGeneration client)
    {
        var w = new PacketWriter(Action25.Code, Action25.PetDecide).I32(gold);
        if (pet is null)
            return w.U16(0).U8(0).Hp(0, client).Hp(0, client).U16(0).U16(0).PStr8("").U8(0).U8(0).U8(0)
                .Bytes(new byte[InfoBlockSizeFor(client)]).ToArray();
        w.U16(pet.NpcId).U8(pet.Level).Hp(maxHp, client).Hp(pet.Hp, client).U16((ushort)maxSp).U16(pet.Sp)
            .PStr8(pet.Name).U8(pet.Amity).U8(0).U8((byte)pet.Equipment.Count);
        foreach (var e in pet.Equipment)
            e.WriteTo(w);
        return w.Bytes(InfoBlock(pet, client)).ToArray();
    }

    /// <summary>Builds the pet info block shown in the pet info popup.</summary>
    /// <param name="pet">The pet.</param>
    /// <param name="client">Client generation, which decides the block size and layout.</param>
    /// <returns>The block.</returns>
    public static byte[] InfoBlock(PetInfo pet, ClientGeneration client)
    {
        var block = new byte[InfoBlockSizeFor(client)];
        void Word(int at, int value) =>
            System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(block.AsSpan(at), (ushort)Math.Clamp(value, 0, ushort.MaxValue));
        Word(0, pet.NpcId);
        int shift;
        if (client == ClientGeneration.Client2014)
        {
            System.Buffers.Binary.BinaryPrimitives.WriteInt32LittleEndian(block.AsSpan(23), pet.Hp);
            shift = 2;
        }
        else
        {
            Word(23, pet.Hp);
            shift = 0;
        }
        Word(25 + shift, pet.Sp);
        Word(27 + shift, pet.Strength);
        Word(29 + shift, pet.Constitution);
        Word(31 + shift, pet.Intelligence);
        Word(33 + shift, pet.Wisdom);
        Word(35 + shift, pet.Agility);
        block[37 + shift] = pet.Level;
        block[45 + shift] = pet.Amity;
        return block;
    }
}
