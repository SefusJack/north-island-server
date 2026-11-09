using NorthIsland.Protocol.IO;
using NorthIsland.Protocol.Opcodes;

namespace NorthIsland.Protocol.Messages.Player;

/// <summary>Builders for general player packets: attributes, money, hotkeys, levels, experience potions and notices.</summary>
public static class PlayerMessages
{
    /// <summary>Sends the player's own attribute record (<c>5/3</c>).</summary>
    /// <param name="attributes">The attributes.</param>
    /// <param name="client">The client generation the record is written for.</param>
    /// <returns>The packet.</returns>
    public static byte[] OwnAttributes(PlayerAttributes attributes, ClientGeneration client)
    {
        var w = new PacketWriter(Action5.Code, Action5.OwnAttributes);
        attributes.WriteTo(w, client);
        return w.ToArray();
    }

    /// <summary>Tells the client the scene is ready (<c>5/4</c>).</summary>
    /// <returns>The packet.</returns>
    public static byte[] SceneReady() => new PacketWriter(Action5.Code, Action5.SceneReady).ToArray();

    /// <summary>Sets the gold the player carries (<c>26/4</c>).</summary>
    /// <param name="gold">The gold amount.</param>
    /// <returns>The packet.</returns>
    public static byte[] Money(int gold) => new PacketWriter(Action26.Code, Action26.SetMoney).I32(gold).ToArray();

    /// <summary>Sends the hotkey bar (<c>40/1</c>).</summary>
    /// <param name="keys">Kind, id, page and slot of each hotkey.</param>
    /// <returns>The packet.</returns>
    public static byte[] Hotkeys(IEnumerable<(byte Kind, ushort Id, byte Page, byte Slot)> keys)
    {
        var w = new PacketWriter(Action40.Code, Action40.Slot);
        foreach (var (kind, id, page, slot) in keys)
            w.U8(kind).U16(id).U8(page).U8(slot);
        return w.ToArray();
    }

    /// <summary>Sets a player's level in the scene (<c>5/18</c>).</summary>
    /// <param name="id">Id of the player.</param>
    /// <param name="level">The level.</param>
    /// <returns>The packet.</returns>
    public static byte[] SceneLevel(int id, byte level) => new PacketWriter(Action5.Code, Action5.SceneLevel).I32(id).U8(level).ToArray();

    /// <summary>Shows the level-up effect for a player.</summary>
    /// <param name="id">Id of the player.</param>
    /// <returns>The packet.</returns>
    public static byte[] LevelUp(int id) => new PacketWriter(Action5.Code, Action5.LevelUp).I32(id).ToArray();

    /// <summary>Starts or ends the New Year event notice.</summary>
    /// <param name="on">True when the event begins, false when it ends.</param>
    /// <returns>The packet.</returns>
    public static byte[] NewYearNotice(bool on) => EventNotice(on ? (byte)95 : (byte)96);

    /// <summary>Shows one of the client's event notices (<c>23/57</c>).</summary>
    /// <param name="notice">The notice number.</param>
    /// <returns>The packet.</returns>
    public static byte[] EventNotice(byte notice) => new PacketWriter(Action23.Code, Action23.EventNotice).U8(notice).ToArray();

    /// <summary>Sends the Lucky 7 counter.</summary>
    /// <param name="count">The count; clamped to 0 to 255.</param>
    /// <returns>The packet.</returns>
    public static byte[] Lucky7Count(int count) => new PacketWriter(Action27.Code, Action27.Lucky7Count).U8((byte)Math.Clamp(count, 0, 255)).ToArray();

    /// <summary>Shows or hides the experience rate icon.</summary>
    /// <param name="active">True to show it.</param>
    /// <param name="rate">The rate times 10.</param>
    /// <returns>The packet.</returns>
    public static byte[] ExpRate(bool active, byte rate) =>
        new PacketWriter(Action5.Code, Action5.ExpPotion).U8(1).U8(active ? (byte)1 : (byte)0).U8(active ? rate : (byte)0).ToArray();

    /// <summary>Refuses an experience potion (<c>5/17</c> kind 2).</summary>
    /// <param name="code">The refusal code.</param>
    /// <returns>The packet.</returns>
    public static byte[] ExpPotionRefused(byte code) =>
        new PacketWriter(Action5.Code, Action5.ExpPotion).U8(ExpPotionKind.Refused).U8(code).ToArray();

    /// <summary>Sends the remaining time of an experience potion (<c>5/17</c> kind 3).</summary>
    /// <param name="active">True when a potion is active.</param>
    /// <param name="minutes">Minutes left.</param>
    /// <param name="rate">The rate times 10.</param>
    /// <returns>The packet.</returns>
    public static byte[] ExpPotionRemaining(bool active, int minutes, byte rate) =>
        new PacketWriter(Action5.Code, Action5.ExpPotion).U8(ExpPotionKind.Remaining).U8(active ? (byte)1 : (byte)0).I32(minutes).U8(rate).ToArray();

    /// <summary>Tells that an experience potion ran out (<c>5/17</c> kind 4).</summary>
    /// <param name="stillActive">True to keep the icon because another event still runs.</param>
    /// <returns>The packet.</returns>
    public static byte[] ExpPotionExpired(bool stillActive) =>
        new PacketWriter(Action5.Code, Action5.ExpPotion).U8(ExpPotionKind.Expired).U8(stillActive ? (byte)1 : (byte)0).ToArray();

    /// <summary>Tells that the money bag is full (<c>26/3</c>).</summary>
    /// <returns>The packet.</returns>
    public static byte[] MoneyBagFull() => new PacketWriter(Action26.Code, Action26.MoneyBagFull).ToArray();

    /// <summary>Adds or takes away gold.</summary>
    /// <param name="amount">Gold to add when positive, to take away when negative.</param>
    /// <returns>The packet.</returns>
    public static byte[] MoneyChange(int amount) => amount >= 0
        ? new PacketWriter(Action26.Code, Action26.AddMoney).I32(amount).ToArray()
        : new PacketWriter(Action26.Code, Action26.SpendMoney).I32(-amount).ToArray();

    /// <summary>Changes a player's role, which sets body type and head (<c>5/12</c>).</summary>
    /// <param name="playerId">Id of the player.</param>
    /// <param name="role">The role byte.</param>
    /// <returns>The packet.</returns>
    public static byte[] RoleChange(int playerId, byte role) => new PacketWriter(Action5.Code, Action5.RoleChange).I32(playerId).U8(role).ToArray();
}
