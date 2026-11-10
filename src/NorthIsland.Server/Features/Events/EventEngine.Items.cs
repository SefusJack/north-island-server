using Microsoft.Extensions.Logging;
using NorthIsland.GameData.Events;
using NorthIsland.Protocol.Messages.Items;
using NorthIsland.Protocol.Messages.Player;
using NorthIsland.Protocol.Opcodes;

namespace NorthIsland.Server.Features.Events;

public sealed partial class EventEngine
{
    /// <summary>The player's line "No more things to collect now."</summary>
    private const ushort NothingToCollectTalk = 50061;

    /// <summary>Random source for gathering: returns a number from 0 up to but not including its argument. Replaceable by tests.</summary>
    public Func<int, int> GatherRoll { get; set; } = GameRandom.Shared.Next;

    /// <summary>Picks what a gathering spot gives.</summary>
    /// <param name="session">The player's session.</param>
    /// <param name="run">The running event, for the log.</param>
    /// <param name="mapped">The mapped result: the spot's kind and tier.</param>
    /// <returns>The item result to give; null when the spot has no pool.</returns>
    private MappedResult? PickGather(GameSession session, EventRun run, MappedResult mapped)
    {
        if (GatherPools.Pick(mapped.Service, mapped.Convey, GatherRoll) is not { } itemId)
        {
            logger.LogWarning("[{Session}] {Source}: {What}: no such pool", session.Id, run.Source, mapped.Description);
            return null;
        }
        return ResultMapper.Map(new EveScriptRecord(0, 1, 1, 1, itemId, 0, 1, 0, 0, 0));
    }

    /// <summary>Teaches a character a skill a script names, and tells the client.</summary>
    /// <param name="session">The player's session.</param>
    /// <param name="character">The character.</param>
    /// <param name="run">The running event, for the log.</param>
    /// <param name="skillId">The skill id.</param>
    private void LearnSkill(GameSession session, Character character, EventRun run, ushort skillId)
    {
        if (skills[skillId] is not { } skill)
        {
            logger.LogWarning("[{Session}] {Source}: skill {Skill} is not in Skill.dat, skipped", session.Id, run.Source, skillId);
            return;
        }
        if (character.Skill(skillId) is not null)
            return;
        character.Skills.Add(new LearnedSkill { Id = skillId });
        session.Send(InnMessages.SetAttribute(Action8.AttrSkillLevel, 1, skillId));
        logger.LogInformation("[{Session}] {Source}: {Name} learns {Skill} ({Id})", session.Id, run.Source, character.Name, skill.Name, skillId);
    }

    /// <summary>Gives the character the items of an item result, or takes them when the count is negative, and tells the client.</summary>
    /// <param name="session">The player's session.</param>
    /// <param name="character">The character.</param>
    /// <param name="run">The running event, for the log.</param>
    /// <param name="mapped">The mapped result: the item id is the wire target and the count the wire value.</param>
    /// <returns>The number of items given; 0 for a take or an unknown item.</returns>
    private int ApplyItem(GameSession session, Character character, EventRun run, MappedResult mapped)
    {
        var itemId = mapped.Wire.Target;
        var count = mapped.Wire.Value;
        if (items[itemId] is null)
        {
            logger.LogWarning("[{Session}] {Source}: item {Item} is not in Item.dat, skipped", session.Id, run.Source, itemId);
            return 0;
        }
        if (count < 0)
        {
            var removed = Bag.Remove(character, itemId, -count);
            for (var left = removed; left > 0; left -= byte.MaxValue)
                session.Send(ItemMessages.BagRemoveById(itemId, (byte)Math.Min(left, byte.MaxValue)));
            logger.LogInformation("[{Session}] {Source}: {What} (took {Removed}, {Left} left)", session.Id, run.Source,
                mapped.Description, removed, Bag.Count(character, itemId));
            return 0;
        }

        var changes = Bag.Add(character, items, itemId, count);
        foreach (var (stack, added) in changes)
            session.Send(ItemMessages.BagAdd(new BagEntry { Slot = stack.Slot, ItemId = itemId, Count = added, Wear = stack.Wear }));
        var given = changes.Sum(c => c.Added);
        logger.LogInformation("[{Session}] {Source}: {What} ({Name}) into slot(s) {Slots}{Full}", session.Id, run.Source, mapped.Description,
            items[itemId]!.Name, string.Join(',', changes.Select(c => c.Item.Slot).Distinct()), given < count ? $", bag full: {count - given} lost" : "");
        return given;
    }
}
