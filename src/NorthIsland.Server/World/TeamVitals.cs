using System.Collections.Immutable;
using NorthIsland.Protocol.Messages.World;
using NorthIsland.Protocol.Opcodes;

namespace NorthIsland.Server.World;

/// <summary>Keeps team mates' bars current: sends a player's HP, SP and the values the maxima come from to the other members of their team.</summary>
/// <param name="world">The online world, for teams and sessions.</param>
/// <param name="combat">The combat stats, for the worn items' max bonuses.</param>
public sealed class TeamVitals(
    WorldState world,
    CombatStats combat)
{
    /// <summary>The attribute ids sent to team mates.</summary>
    public static readonly ImmutableArray<byte> Attributes =
        [
            Action8.AttrReborn, Action8.AttrLevel, Action8.AttrJob, Action8.AttrCon, Action8.AttrWis, CombatStats.AttrMaxHpBonus, CombatStats.AttrMaxSpBonus,
            Action8.AttrMaxHpExtra, Action8.AttrMaxSpExtra, Action8.AttrHp, Action8.AttrSp,
        ];

    /// <summary>Hooks the team vitals into the world: own attribute changes go to the team, and an arriving player is introduced.</summary>
    public void Register()
    {
        world.OwnAttributeSent += OnOwnAttribute;
        world.Arrived += Introduce;
    }

    /// <summary>The value of one of the team attributes for a character.</summary>
    /// <param name="c">The character.</param>
    /// <param name="attr">The attribute id.</param>
    /// <returns>The value; 0 for an attribute not in the list.</returns>
    public int ValueOf(Character c, byte attr) => attr switch
    {
        Action8.AttrReborn => c.RebornCount, // reborn count
        Action8.AttrLevel => c.Level, // level
        Action8.AttrJob => c.Job, // job
        Action8.AttrCon => c.Constitution, // constitution
        Action8.AttrWis => c.Wisdom, // wisdom
        CombatStats.AttrMaxHpBonus or CombatStats.AttrMaxSpBonus => combat.MaxBonus(c, attr), // max HP and max SP bonuses, from the combat stats
        Action8.AttrHp => c.Hp, // current HP
        Action8.AttrSp => c.Sp, // current SP
        _ => 0, // any other attribute: 0
    };

    /// <summary>The packets that give team mates all the team attributes of a player.</summary>
    /// <param name="player">The player's session.</param>
    /// <returns>One packet per attribute.</returns>
    public IEnumerable<byte[]> Snapshot(GameSession player) =>
        Attributes.Select(a => TeamMessages.MateAttribute(player.Account!.LoginId, a, ValueOf(player.ActiveCharacter!, a)));

    /// <summary>Gives a player and their online team mates each other's current values.</summary>
    /// <param name="member">The player's session.</param>
    public void Introduce(GameSession member)
    {
        if (member.Account is null || member.ActiveCharacter is null)
            return;
        var id = member.Account.LoginId;
        foreach (var other in world.Teams.TeamOf(id).Where(t => t != id).Select(t => world.SessionOf(t)).OfType<GameSession>())
        {
            if (other.ActiveCharacter is null)
                continue;
            foreach (var packet in Snapshot(other))
                member.Send(packet);
            foreach (var packet in Snapshot(member))
                other.Send(packet);
        }
    }

    /// <summary>Relays an attribute change of a player to their team mates when it is one of the team attributes.</summary>
    /// <param name="session">The player's session.</param>
    /// <param name="attr">The attribute id.</param>
    /// <param name="value">The new value.</param>
    /// <param name="value2">The second value of the attribute packet.</param>
    private void OnOwnAttribute(GameSession session, byte attr, int value, int value2)
    {
        if (session.Account is not { } account || !Attributes.Contains(attr))
            return;
        var packet = TeamMessages.MateAttribute(account.LoginId, attr, value, value2);
        foreach (var id in world.Teams.TeamOf(account.LoginId).Where(t => t != account.LoginId))
            if (world.SessionOf(id) is { } mate)
                mate.Send(packet);
    }
}
