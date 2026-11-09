using Microsoft.Extensions.Logging;
using NorthIsland.Protocol.IO;
using NorthIsland.Protocol.Messages.Items;
using NorthIsland.Protocol.Opcodes;

namespace NorthIsland.Server.Features.Vehicles;

/// <summary>Handles the Fuel Bank item: four rows of stored fuel per player that can be filled, emptied or used on the ridden vehicle.</summary>
/// <param name="items">The item records, used to check that an item is fuel.</param>
/// <param name="care">Refuels the ridden vehicle.</param>
/// <param name="logger">Logger for the handler's messages.</param>
public sealed class FuelBankHandlers(
    ItemData items,
    VehicleCareHandlers care,
    ILogger<FuelBankHandlers> logger)
{
    /// <summary>Item id of the Fuel Bank.</summary>
    public const ushort FuelBank = 34399; // Fuel Bank

    /// <summary>The number of rows in the bank (<c>Rows</c>) and the most fuel items one row holds (<c>RowMax</c>).</summary>
    public const int Rows = 4, RowMax = 250;

    /// <summary>Sends every non-empty fuel row to the client when the player enters the game.</summary>
    /// <param name="session">The player's session.</param>
    /// <param name="c">The player's character.</param>
    public static void SendOnLogin(GameSession session, Character c)
    {
        foreach (var row in c.FuelTank.Where(r => r.Count > 0))
            session.Send(Notice(1, row));
    }

    /// <summary>Builds the packet that tells the client what a fuel row now holds.</summary>
    /// <param name="kind">1 after a deposit or at login, 2 after fuel was taken out or used.</param>
    /// <param name="row">The row to report; the count is capped at 255.</param>
    /// <returns>The packet.</returns>
    private static byte[] Notice(byte kind, FuelRow row) =>
        new PacketWriter(Action23.Code, Action23.FuelBankNotice).U8(kind).U8(row.Row).U16(row.ItemId).U8((byte)Math.Min(255, row.Count)).ToArray();

    /// <summary>Handles an action on a Fuel Bank in the bag: the option byte is 1 to put fuel in, 2 to take it out, 3 to use it.</summary>
    /// <param name="session">The player's session.</param>
    /// <param name="body">The option, the bag slot of the Fuel Bank, then the arguments of that option.</param>
    internal ValueTask OnBank(GameSession session, ReadOnlyMemory<byte> body)
    {
        if (session.State != SessionState.InWorld || session.ActiveCharacter is not { } c || session.Battle is not null || body.Length < 3)
            return ValueTask.CompletedTask;
        var (option, bankSlot) = (body.Span[0], body.Span[1]);
        if (c.Bag.Find(b => b.Slot == bankSlot)?.ItemId != FuelBank)
            return ValueTask.CompletedTask;
        switch (option)
        {
            case 1 when body.Length >= 4: // put
                Put(session, c, body.Span[2], body.Span[3]); // 2 bagSlot, 3 rowNo
                break;
            case 2 when body.Length >= 4: // take
                Take(session, c, body.Span[2], body.Span[3]); // 2 rowNo, 3 n
                break;
            case 3: // use
                Use(session, c, body.Span[2]); // rowNo
                break;
        }
        return ValueTask.CompletedTask;
    }

    /// <summary>Moves fuel items from a bag stack into a row, up to the row maximum and only onto a row that holds the same fuel.</summary>
    /// <param name="session">The player's session.</param>
    /// <param name="c">The player's character.</param>
    /// <param name="bagSlot">The bag slot of the fuel stack.</param>
    /// <param name="rowNo">The row number, 1 to 4.</param>
    private void Put(GameSession session, Character c, byte bagSlot, byte rowNo)
    {
        var stack = c.Bag.Find(b => b.Slot == bagSlot);
        if (stack is null || items[stack.ItemId] is not { SubType: 121 } || rowNo is < 1 or > Rows)
            return;
        var row = c.FuelTank.Find(r => r.Row == rowNo);
        if (row is not null && row.Count > 0 && row.ItemId != stack.ItemId)
            return;
        row ??= AddRow(c, rowNo, stack.ItemId);
        var moved = Math.Min(stack.Count, RowMax - row.Count);
        if (moved <= 0)
            return;
        row.ItemId = stack.ItemId;
        row.Count += moved;
        stack.Count -= (byte)moved;
        session.Send(ItemMessages.BagSlotDecrease(stack.Slot, (byte)moved));
        if (stack.Count == 0)
            c.Bag.Remove(stack);
        session.Send(Notice(1, row));
        logger.LogInformation("[{Session}] {Name} stores {Count} of {Item} in fuel row {Row}", session.Id, c.Name, moved, row.ItemId, rowNo);
    }

    /// <summary>Moves up to 50 fuel items from a row into the bag, as many as fit.</summary>
    /// <param name="session">The player's session.</param>
    /// <param name="c">The player's character.</param>
    /// <param name="rowNo">The row number.</param>
    /// <param name="n">How many items the player asked for.</param>
    private void Take(GameSession session, Character c, byte rowNo, byte n)
    {
        var row = c.FuelTank.Find(r => r.Row == rowNo);
        if (row is null || row.Count <= 0 || n == 0)
            return;
        var want = Math.Min(Math.Min((int)n, 50), row.Count);
        var added = 0;
        foreach (var (stack, got) in Bag.Add(c, items, row.ItemId, want))
        {
            session.Send(ItemMessages.BagAdd(new BagEntry { Slot = stack.Slot, ItemId = stack.ItemId, Count = got, Wear = stack.Wear }));
            added += got;
        }
        if (added == 0)
            return;
        row.Count -= added;
        session.Send(Notice(2, row));
        if (row.Count == 0)
            c.FuelTank.Remove(row);
        logger.LogInformation("[{Session}] {Name} takes {Count} of {Item} from fuel row {Row}", session.Id, c.Name, added, row.ItemId, rowNo);
    }

    /// <summary>Uses one fuel item of a row on the ridden vehicle.</summary>
    /// <param name="session">The player's session.</param>
    /// <param name="c">The player's character.</param>
    /// <param name="rowNo">The row number.</param>
    private void Use(GameSession session, Character c, byte rowNo)
    {
        var row = c.FuelTank.Find(r => r.Row == rowNo);
        if (row is null || row.Count <= 0 || !care.Refuel(session, c, row.ItemId))
            return;
        row.Count--;
        session.Send(Notice(2, row));
        if (row.Count == 0)
            c.FuelTank.Remove(row);
    }

    /// <summary>Adds an empty row for a fuel item to the character's bank.</summary>
    /// <param name="c">The player's character.</param>
    /// <param name="rowNo">The row number.</param>
    /// <param name="item">The fuel item id.</param>
    /// <returns>The new row.</returns>
    private static FuelRow AddRow(Character c, byte rowNo, ushort item)
    {
        var row = new FuelRow { Row = rowNo, ItemId = item };
        c.FuelTank.Add(row);
        return row;
    }
}
