using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NorthIsland.GameData.Formulas;
using NorthIsland.Protocol.IO;
using NorthIsland.Protocol.Messages.Player;
using NorthIsland.Protocol.Messages.World;
using NorthIsland.Protocol.Opcodes;

namespace NorthIsland.Server.Features.Houses;

/// <summary>Life in the house: bathing in a tub to restore HP and SP, and training on the Treadmill or the Bicep Exerciser.</summary>
/// <param name="world">The world state, to find the sessions in a house.</param>
/// <param name="items">The item data, for names and sub types.</param>
/// <param name="experience">The experience service, for the training gain.</param>
/// <param name="formulas">The formula data, for the experience a level needs.</param>
/// <param name="tents">The house and furniture handlers, for house owners and the house entry event.</param>
/// <param name="options">The house options, for the bath and training numbers.</param>
/// <param name="logger">The logger.</param>
public sealed class HouseLifeHandlers(
    WorldState world,
    ItemData items,
    Experience experience,
    FormulaData formulas,
    TentHandlers tents,
    IOptions<HouseOptions> options,
    ILogger<HouseLifeHandlers> logger)
{
    /// <summary>The item id of the Treadmill, which trains the active pet.</summary>
    public const ushort Treadmill = 39105; // Treadmill

    /// <summary>The item id of the Bicep Exerciser, which trains the player.</summary>
    public const ushort BicepExerciser = 39100; // Bicep Exerciser

    /// <summary>The item id of the Screen, which speeds up the HP recovery in a bath.</summary>
    public const ushort Screen = 39091; // Screen

    /// <summary>The item id of the Tiger-fur Carpet, which speeds up the SP recovery in a bath.</summary>
    public const ushort TigerFurCarpet = 39083; // Tiger-fur Carpet

    /// <summary>The item sub type of bathtubs.</summary>
    public const byte BathtubSubType = 112;

    /// <summary>Hooks the house entry event so the owner is shown their training progress.</summary>
    internal void Start()
    {
        tents.HouseEntered += OnHouseEntered;
    }

    /// <summary>The login id of a session's account, which is also their id in the house packets.</summary>
    /// <param name="session">The session.</param>
    /// <returns>The login id.</returns>
    private static int HumId(GameSession session) => session.Account!.LoginId;

    /// <summary>The active character of a session that is in the world and stands in their own house.</summary>
    /// <param name="session">The session.</param>
    /// <returns>The character, or null when the session is not in the world or not in its own house.</returns>
    private static Character? InOwnHouse(GameSession session)
    {
        var character = session.ActiveCharacter;
        return session.State == SessionState.InWorld && character is not null && session.Account is not null
            && TentHandlers.HouseMapOf(session.Account.LoginId) == character.MapId ? character : null;
    }

    /// <summary>Sends a packet to the players in the house of a session.</summary>
    /// <param name="session">The session in the house.</param>
    /// <param name="packet">The packet.</param>
    /// <param name="includeSelf">False to leave the session itself out.</param>
    private void SendToHouse(GameSession session, byte[] packet, bool includeSelf = true)
    {
        foreach (var s in world.OnMap(session.ActiveCharacter!.MapId).Append(session).Distinct())
            if (includeSelf || s != session)
                s.Send(packet);
    }

    /// <summary>Handles <c>23/55</c>: the owner gets into a tub of their house and starts to bathe.</summary>
    /// <param name="session">The owner's session.</param>
    /// <param name="body">The house owner's login id (four bytes) and the furniture index (two bytes).</param>
    internal ValueTask OnBathe(GameSession session, ReadOnlyMemory<byte> body)
    {
        if (InOwnHouse(session) is not { } c || body.Length < 6 || session.Battle is not null || session.HouseLife.BathFurn != 0)
            return ValueTask.CompletedTask;
        var r = new PacketReader(body.Span);
        var (owner, furn) = (r.I32(), r.U16());
        if (owner != HumId(session) || c.Furniture.Find(f => f.Index == furn) is not { } piece || items[piece.ItemId]?.SubType != BathtubSubType)
        {
            logger.LogInformation("[{Session}] {Name}: bathing refused (owner {Owner}, furniture {Furn})", session.Id, c.Name, owner, furn);
            return ValueTask.CompletedTask;
        }
        session.HouseLife.BathFurn = furn;
        session.HouseLife.BathNext = GameClock.UtcNow.AddSeconds(options.Value.HouseLife.BathSeconds);
        SendToHouse(session, HouseLifeMessages.Bathing(HumId(session), furn, 1));
        logger.LogInformation("[{Session}] {Name} bathes in {Tub}", session.Id, c.Name, items[piece.ItemId]?.Name);
        return ValueTask.CompletedTask;
    }

    /// <summary>Handles <c>23/56</c>: the player gets out of the tub.</summary>
    /// <param name="session">The player's session.</param>
    /// <param name="body">The packet body, not used.</param>
    internal ValueTask OnStopBathe(GameSession session, ReadOnlyMemory<byte> body)
    {
        if (session.HouseLife.BathFurn != 0 && session.ActiveCharacter is not null)
            EndBath(session);
        return ValueTask.CompletedTask;
    }

    /// <summary>Ends the bath and tells the house.</summary>
    /// <param name="session">The bathing player's session.</param>
    private void EndBath(GameSession session)
    {
        session.HouseLife.BathFurn = 0;
        SendToHouse(session, HouseLifeMessages.BatheEnd(HumId(session), 1));
    }

    /// <summary>Does one bath step when it is due: restores HP and SP, and ends the bath when the tub or the house is gone.</summary>
    /// <param name="session">The bathing player's session.</param>
    /// <param name="c">The player's character.</param>
    /// <param name="now">The current time.</param>
    private void BathTick(GameSession session, Character c, DateTime now)
    {
        var o = options.Value.HouseLife;
        if (c.Furniture.Find(f => f.Index == session.HouseLife.BathFurn) is null || TentHandlers.HouseMapOf(HumId(session)) != c.MapId)
        {
            EndBath(session);
            return;
        }
        if (now < session.HouseLife.BathNext)
            return;
        session.HouseLife.BathNext = now.AddSeconds(Math.Max(1, o.BathSeconds));
        var hpFactor = c.Furniture.Exists(f => f.ItemId == Screen) ? o.ScreenHpFactor : 1;
        var spFactor = c.Furniture.Exists(f => f.ItemId == TigerFurCarpet) ? o.CarpetSpFactor : 1;
        var spa = TimeIconHandlers.BathFactor(c);
        var hp = Math.Min(c.MaxHp, c.Hp + Math.Max(1, (int)Math.Round(c.MaxHp * o.BathHpPercent / 100.0 * hpFactor * spa)));
        var sp = Math.Min(c.MaxSp, c.Sp + Math.Max(1, (int)Math.Round(c.MaxSp * o.BathSpPercent / 100.0 * spFactor * spa)));
        if (hp != c.Hp)
        {
            c.Hp = hp;
            session.Send(InnMessages.SetAttribute(Action8.AttrHp, hp));
        }
        if (sp != c.Sp)
        {
            c.Sp = sp;
            session.Send(InnMessages.SetAttribute(Action8.AttrSp, sp));
        }
    }

    /// <summary>
    /// Handles <c>23/71</c>: a player sat on a piece or started on a training machine; others in the house are told and a training starts on the
    /// owner's machine.
    /// </summary>
    /// <param name="session">The player's session.</param>
    /// <param name="body">The furniture index (two bytes) and the seat.</param>
    internal ValueTask OnUse(GameSession session, ReadOnlyMemory<byte> body)
    {
        if (session.State != SessionState.InWorld || session.ActiveCharacter is not { } c || body.Length < 3)
            return ValueTask.CompletedTask;
        var r = new PacketReader(body.Span);
        var (furn, seat) = (r.U16(), r.U8());
        var owner = TentHandlers.IsHouseMap(c.MapId) ? tents.OwnerOfHouse(c.MapId)?.Character : null;
        var piece = owner?.Furniture.Find(f => f.Index == furn);
        if (piece is null)
            return ValueTask.CompletedTask;
        SendToHouse(session, HouseLifeMessages.FurnitureUsed(HumId(session), furn, seat), includeSelf: false);
        if (piece.ItemId is not (Treadmill or BicepExerciser) || InOwnHouse(session) is null)
            return ValueTask.CompletedTask;

        var o = options.Value.HouseLife;
        byte petSlot = 0;
        if (piece.ItemId == Treadmill)
        {
            var pet = c.Pets.Find(p => p.NpcId == c.ActivePetId);
            if (pet is null)
                return ValueTask.CompletedTask;
            petSlot = pet.Slot;
            ResetIfLeveled(session, pet);
        }
        else
            ResetIfLeveled(session, c);
        session.HouseLife.Training = new TrainingState(furn, petSlot, GameClock.UtcNow.AddSeconds(Math.Max(1, o.TrainSeconds)));
        logger.LogInformation("[{Session}] {Name} trains on {Piece}{Pet}", session.Id, c.Name, items[piece.ItemId]?.Name,
            petSlot == 0 ? "" : $" with pet slot {petSlot}");
        return ValueTask.CompletedTask;
    }

    /// <summary>Handles <c>23/72</c>: the player got up from a piece, which ends a training.</summary>
    /// <param name="session">The player's session.</param>
    /// <param name="body">The packet body, not used.</param>
    internal ValueTask OnStopUse(GameSession session, ReadOnlyMemory<byte> body)
    {
        if (session.State != SessionState.InWorld || session.ActiveCharacter is null)
            return ValueTask.CompletedTask;
        session.HouseLife.Training = null;
        SendToHouse(session, HouseLifeMessages.FurnitureLeft(HumId(session)), includeSelf: false);
        return ValueTask.CompletedTask;
    }

    /// <summary>Resets the training progress of a character who gained a level since it was set.</summary>
    /// <param name="session">The player's session.</param>
    /// <param name="c">The character.</param>
    private static void ResetIfLeveled(GameSession session, Character c)
    {
        if (c.TrainPercent != 0 && c.TrainLevel != c.Level)
        {
            c.TrainPercent = 0;
            session.Send(HouseLifeMessages.TrainingReset());
        }
    }

    /// <summary>Resets the training progress of a pet that gained a level since it was set.</summary>
    /// <param name="session">The owner's session.</param>
    /// <param name="pet">The pet.</param>
    private static void ResetIfLeveled(GameSession session, Pet pet)
    {
        if (pet.TrainPercent != 0 && pet.TrainLevel != pet.Level)
        {
            pet.TrainPercent = 0;
            session.Send(HouseLifeMessages.PetTrainingReset(pet.Slot));
        }
    }

    /// <summary>
    /// Does one training step when it is due: adds experience and progress to the player or the pet, and ends the training at 100 percent or at the
    /// top level.
    /// </summary>
    /// <param name="session">The training player's session.</param>
    /// <param name="c">The player's character.</param>
    /// <param name="now">The current time.</param>
    private void TrainTick(GameSession session, Character c, DateTime now)
    {
        var training = session.HouseLife.Training!;
        var o = options.Value.HouseLife;
        if (!TentHandlers.IsHouseMap(c.MapId) || TentHandlers.HouseMapOf(HumId(session)) != c.MapId || c.Furniture.Find(f => f.Index == training.Furn) is null
            || formulas.Table is not { } f)
        {
            session.HouseLife.Training = null;
            return;
        }
        if (now < training.Next)
            return;
        training.Next = now.AddSeconds(Math.Max(1, o.TrainSeconds));

        if (training.PetSlot == 0)
        {
            ResetIfLeveled(session, c);
            if (c.Level >= FormulaTable.MaxLevel(c.RebornCount))
            {
                EndTraining(session, 1);
                return;
            }
            var gain = Math.Max(1, (int)Math.Round(f.LevelUpExp(c.Level, c.RebornCount) * o.TrainPercent / 100.0));
            var level = c.Level;
            experience.Gain(session, gain);
            if (c.Level != level)
            {
                (c.TrainPercent, c.TrainLevel) = (0, c.Level);
                session.Send(HouseLifeMessages.TrainingReset());
                return;
            }
            c.TrainLevel = c.Level;
            c.TrainPercent = (byte)Math.Min(100, c.TrainPercent + o.TrainPercent);
            session.Send(HouseLifeMessages.TrainingPercent(c.TrainPercent, expUp: true));
            if (c.TrainPercent >= 100)
                EndTraining(session, 1);
            return;
        }

        if (c.Pets.Find(p => p.Slot == training.PetSlot) is not { } pet || pet.NpcId != c.ActivePetId)
        {
            session.HouseLife.Training = null;
            return;
        }
        ResetIfLeveled(session, pet);
        if (pet.Level >= FormulaTable.MaxLevel(pet.Reborn))
        {
            EndTraining(session, 2); // flag
            return;
        }
        var petGain = Math.Max(1, (int)Math.Round(f.LevelUpExp(pet.Level, pet.Reborn) * o.TrainPercent / 100.0));
        var petLevel = pet.Level;
        pet.Exp = (int)Math.Min(int.MaxValue, (long)pet.Exp + petGain);
        session.Send(PetMessages.SetAttribute(pet.Slot, Action8.AttrExp, pet.Exp));
        experience.PetGains(session, pet, petGain);
        if (pet.Level != petLevel)
        {
            (pet.TrainPercent, pet.TrainLevel) = (0, pet.Level);
            session.Send(HouseLifeMessages.PetTrainingReset(pet.Slot));
            return;
        }
        pet.TrainLevel = pet.Level;
        pet.TrainPercent = (byte)Math.Min(100, pet.TrainPercent + o.TrainPercent);
        session.Send(HouseLifeMessages.PetTrainingPercent(pet.TrainPercent, pet.Slot, expUp: true));
        if (pet.TrainPercent >= 100)
            EndTraining(session, 2); // flag
    }

    /// <summary>Ends the training, tells the player why and shows the house that they got up.</summary>
    /// <param name="session">The player's session.</param>
    /// <param name="flag">1 when the player was trained, 2 when the pet was trained.</param>
    private void EndTraining(GameSession session, byte flag)
    {
        session.HouseLife.Training = null;
        session.Send(HouseLifeMessages.TrainingOver(HumId(session), flag));
        SendToHouse(session, HouseLifeMessages.FurnitureLeft(HumId(session)), includeSelf: false);
    }

    /// <summary>Shows the owner their training progress when they enter their house.</summary>
    /// <param name="session">The session that entered the house.</param>
    /// <param name="owner">The house owner's session, or null when the owner is not online.</param>
    private void OnHouseEntered(GameSession session, GameSession? owner)
    {
        if (owner != session || session.ActiveCharacter is not { } c)
            return;
        if (c.TrainPercent != 0 && c.TrainLevel == c.Level)
            session.Send(HouseLifeMessages.TrainingPercent(c.TrainPercent, expUp: false));
        foreach (var pet in c.Pets.Where(p => p.TrainPercent != 0 && p.TrainLevel == p.Level))
            session.Send(HouseLifeMessages.PetTrainingPercent(pet.TrainPercent, pet.Slot, expUp: false));
    }

    /// <summary>Runs the bath and training steps of a session that are due.</summary>
    /// <param name="session">The player's session.</param>
    /// <param name="c">The player's character.</param>
    /// <param name="now">The current time.</param>
    public void Tick(GameSession session, Character c, DateTime now)
    {
        if (session.HouseLife.BathFurn != 0)
            BathTick(session, c, now);
        if (session.HouseLife.Training is not null)
            TrainTick(session, c, now);
    }

    /// <summary>The sessions that are online.</summary>
    /// <returns>The online sessions.</returns>
    public IEnumerable<GameSession> Online() => world.OnlineSessions();
}
