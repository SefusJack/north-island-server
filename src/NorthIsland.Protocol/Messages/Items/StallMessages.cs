using NorthIsland.Protocol.IO;
using NorthIsland.Protocol.Messages.Player;
using NorthIsland.Protocol.Opcodes;

namespace NorthIsland.Protocol.Messages.Items;

/// <summary>Builders for the packets of player stalls (item stalls, utensil stalls and pet stalls), all server to client.</summary>
public static class StallMessages
{
    /// <summary>Size in bytes of one item record in <c>23/33</c> for the 2010 client.</summary>
    public const int ItemRecordSize = 31;

    /// <summary>Size in bytes of one item record in <c>23/33</c> for the 2014 client.</summary>
    public const int ItemRecordSize2014 = 37;

    /// <summary>Opens the owner's own item stall (<c>23/30</c>).</summary>
    /// <param name="name">The stall's name.</param>
    /// <param name="sign">The stall's sign byte.</param>
    /// <param name="items">Bag slot and price of each item on sale.</param>
    /// <returns>The packet.</returns>
    public static byte[] Opened(string name, byte sign, IEnumerable<(byte BagSlot, int Price)> items)
    {
        var w = new PacketWriter(Action23.Code, Action23.StallOpen).PStr8(name).U8(sign);
        foreach (var (slot, price) in items)
            w.U8(slot).I32(price);
        return w.ToArray();
    }

    /// <summary>Lists the item stalls in the scene (<c>23/31</c>).</summary>
    /// <param name="stalls">Owner id, stall name and sign of each stall.</param>
    /// <returns>The packet.</returns>
    public static byte[] List(IEnumerable<(int PlayerId, string Name, byte Sign)> stalls)
    {
        var w = new PacketWriter(Action23.Code, Action23.StallList);
        foreach (var (id, name, sign) in stalls)
            w.I32(id).PStr8(name).U8(sign);
        return w.ToArray();
    }

    /// <summary>Closes an item stall (<c>23/32</c>), for the owner too.</summary>
    /// <param name="playerId">Id of the stall's owner.</param>
    /// <returns>The packet.</returns>
    public static byte[] Closed(int playerId) => new PacketWriter(Action23.Code, Action23.StallClosed).I32(playerId).ToArray();

    /// <summary>Sends the contents of an item stall to a buyer (<c>23/33</c>).</summary>
    /// <param name="ownerId">Id of the stall's owner.</param>
    /// <param name="items">The items on sale.</param>
    /// <param name="client">The client generation the records are written for.</param>
    /// <returns>The packet.</returns>
    public static byte[] Items(int ownerId, IEnumerable<StallItem> items, ClientGeneration client)
    {
        var w = new PacketWriter(Action23.Code, Action23.StallItems).I32(ownerId).I32(0).I32(0).I32(0).U8(0);
        foreach (var i in items)
        {
            w.U8(i.BagSlot).U16(i.ItemId).U8(i.Count).U8(i.Wear).I32(i.Price)
                .U8(0).U8(0).U16(0).F64(0).U8(0).U8(0).U16(0).U32(i.BagSlot).U16(0);
            if (client == ClientGeneration.Client2014)
                w.I32(0).U16(0);
        }
        return w.ToArray();
    }

    /// <summary>Size in bytes of one pet record in <c>23/113</c> for the 2010 client.</summary>
    public const int PetRecordSize = 467;

    /// <summary>Size in bytes of one pet record in <c>23/113</c> for the 2014 client.</summary>
    public const int PetRecordSize2014 = 218;

    /// <summary>Opens the owner's own utensil stall (<see cref="Action23.UtensilStallOpened"/>).</summary>
    /// <param name="name">The stall's name.</param>
    /// <param name="sign">The stall's sign byte.</param>
    /// <param name="goods">Bag slot and price of each item on sale.</param>
    /// <returns>The packet.</returns>
    public static byte[] UtensilOpened(string name, byte sign, IEnumerable<(byte BagSlot, int Price)> goods)
    {
        var w = new PacketWriter(Action23.Code, Action23.UtensilStallOpened).PStr8(name).U8(sign);
        foreach (var (slot, price) in goods)
            w.U8(slot).I32(price);
        return w.ToArray();
    }

    /// <summary>Lists the utensil stalls in the scene.</summary>
    /// <param name="stalls">Owner id, stall name and sign of each stall.</param>
    /// <returns>The packet.</returns>
    public static byte[] UtensilList(IEnumerable<(int PlayerId, string Name, byte Sign)> stalls)
    {
        var w = new PacketWriter(Action23.Code, Action23.UtensilStallList);
        foreach (var (id, name, sign) in stalls)
            w.I32(id).PStr8(name).U8(sign);
        return w.ToArray();
    }

    /// <summary>Closes a utensil stall, for the owner too.</summary>
    /// <param name="playerId">Id of the stall's owner.</param>
    /// <returns>The packet.</returns>
    public static byte[] UtensilClosed(int playerId) => new PacketWriter(Action23.Code, Action23.UtensilStallClosed).I32(playerId).ToArray();

    /// <summary>Opens the owner's own pet stall (<c>23/110</c>).</summary>
    /// <param name="name">The stall's name.</param>
    /// <param name="sign">The stall's sign byte.</param>
    /// <param name="pets">Pet slot and price of each pet on sale.</param>
    /// <returns>The packet.</returns>
    public static byte[] PetStallOpened(string name, byte sign, IEnumerable<(byte PetSlot, int Price)> pets)
    {
        var w = new PacketWriter(Action23.Code, Action23.PetStallOpened).PStr8(name).U8(sign);
        foreach (var (slot, price) in pets)
            w.U8(slot).I32(price);
        return w.ToArray();
    }

    /// <summary>Lists the pet stalls in the scene (<c>23/111</c>).</summary>
    /// <param name="stalls">Owner id, stall name and sign of each stall.</param>
    /// <returns>The packet.</returns>
    public static byte[] PetStallList(IEnumerable<(int PlayerId, string Name, byte Sign)> stalls)
    {
        var w = new PacketWriter(Action23.Code, Action23.PetStallList);
        foreach (var (id, name, sign) in stalls)
            w.I32(id).PStr8(name).U8(sign);
        return w.ToArray();
    }

    /// <summary>Closes a pet stall (<c>23/112</c>), for the owner too.</summary>
    /// <param name="playerId">Id of the stall's owner.</param>
    /// <returns>The packet.</returns>
    public static byte[] PetStallClosed(int playerId) => new PacketWriter(Action23.Code, Action23.PetStallClosed).I32(playerId).ToArray();

    /// <summary>Sends the pets of a pet stall to a buyer (<c>23/113</c>), one fixed-size record per pet.</summary>
    /// <param name="ownerId">Id of the stall's owner.</param>
    /// <param name="pets">Pet slot, price, pet data and the HP and SP maximums of each pet on sale.</param>
    /// <param name="client">The client generation the records are written for.</param>
    /// <returns>The packet.</returns>
    public static byte[] PetStallOffer(int ownerId, IEnumerable<(byte PetSlot, int Price, PetInfo Pet, int MaxHp, int MaxSp)> pets, ClientGeneration client)
    {
        var w = new PacketWriter(Action23.Code, Action23.PetStallOffer).I32(ownerId);
        var size = client == ClientGeneration.Client2014 ? PetRecordSize2014 : PetRecordSize;
        foreach (var (slot, price, pet, maxHp, maxSp) in pets)
        {
            var record = new PacketWriter();
            record.U8(slot).U16(pet.NpcId).I32(price).U8(pet.Level).Hp(pet.Hp, client).Hp(maxHp, client)
                .U16((ushort)Math.Clamp(maxSp, 0, ushort.MaxValue));
            for (var i = 0; i < 6; i++)
            {
                if (i < pet.Equipment.Count)
                    pet.Equipment[i].WriteTo(record);
                else
                    record.Bytes(new byte[PetMessages.EquipmentRecordSize]);
            }
            record.Bytes(PetTradeMessages.InfoBlock(pet, client));
            var bytes = record.ToArray();
            w.Bytes(bytes).Bytes(new byte[size - bytes.Length]);
        }
        return w.ToArray();
    }

    /// <summary>Tells the seller that a pet of the stall was sold (<c>23/114</c>).</summary>
    /// <param name="npcId">Npc id of the sold pet.</param>
    /// <param name="gold">Gold paid.</param>
    /// <returns>The packet.</returns>
    public static byte[] PetSold(ushort npcId, int gold) =>
        new PacketWriter(Action23.Code, Action23.PetStallSale).U8(1).U16(npcId).U8(0).I32(gold).ToArray();

    /// <summary>Tells the seller that items of the stall were sold (<c>23/34</c>).</summary>
    /// <param name="itemId">Id of the sold item.</param>
    /// <param name="count">Number of items sold.</param>
    /// <param name="gold">Gold paid.</param>
    /// <param name="bagSlot">Bag slot of the stack in the seller's bag.</param>
    /// <returns>The packet.</returns>
    public static byte[] Sold(ushort itemId, byte count, int gold, byte bagSlot) =>
        new PacketWriter(Action23.Code, Action23.StallSale).U8(1).U16(itemId).U8(count).I32(gold).U8(bagSlot).ToArray();
}
