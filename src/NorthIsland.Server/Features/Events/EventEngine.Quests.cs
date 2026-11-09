using NorthIsland.GameData.Events;
using NorthIsland.Protocol.Messages.Events;

namespace NorthIsland.Server.Features.Events;

public sealed partial class EventEngine
{
    /// <summary>The item id of the Star, which quests pay.</summary>
    public const ushort StarItem = 30025; // Star

    /// <summary>Tells whether a group changes the state of an instance: an instance goal result, or a battle condition that takes part in one.</summary>
    /// <param name="group">The condition group.</param>
    /// <returns>True when it does.</returns>
    public static bool MovesInstance(ConditionGroup group) =>
        group.Results.Any(r => r.Kind == 17) || group.Conditions.Any(c => c is { Kind: 4, SubKind: 1, Arg1: 1 });

    /// <summary>Applies a quest mark result to the character and tells the client what changed.</summary>
    /// <param name="session">The player's session.</param>
    /// <param name="character">The character.</param>
    /// <param name="r">The mark result record.</param>
    /// <returns>True when a quest flag was newly set.</returns>
    private bool ApplyMark(GameSession session, Character character, EveScriptRecord r)
    {
        var id = r.SubKind;
        var mark = character.Marks.Find(m => m.Id == id);
        var current = character.MarkCount(id);
        var target = r.Arg1 switch
        {
            1 => Math.Clamp(current + r.Value, 0, 255), // add Value to the counter, kept between 0 and 255
            2 => 0, // delete the mark: back to 0
            3 when r.Value2 == 1 && r.Arg2 is >= 1 and <= 8 => r.Value != 0 ? current | (1 << (r.Arg2 - 1)) : current & ~(1 << (r.Arg2 - 1)), // set/clear a bit
            _ => Math.Clamp(r.Value, 0, 255), // any other code: set the counter to Value (0 to 255)
        };
        var hadFlag = character.MarkFlags.Contains(id);
        if (marks.TrySetFlag(session, character, id, r.Arg1 == 1 ? r.Value > 0 : target > 0))
            return !hadFlag && character.MarkFlags.Contains(id);
        var keep = r.Arg1 != 2;
        if (target == current && (mark is not null) == keep)
            return false;

        if (target == 0)
        {
            if (keep)
            {
                if (mark is null)
                    character.Marks.Add(new QuestMark { Id = id, Count = 0 });
                else
                    mark.Count = 0;
            }
            else if (mark is not null)
                character.Marks.Remove(mark);
            if (current > 0)
                session.Send(MarkMessages.Delete(id));
            return false;
        }
        if (mark is null)
            character.Marks.Add(mark = new QuestMark { Id = id });
        mark.Count = (byte)target;
        if (target > current)
        {
            session.Send(MarkMessages.Add(id, (byte)(target - current)));
        }
        else
        {
            session.Send(MarkMessages.Delete(id));
            session.Send(MarkMessages.Add(id, (byte)target));
        }
        return false;
    }
}
