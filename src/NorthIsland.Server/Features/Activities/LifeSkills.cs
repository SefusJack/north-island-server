using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NorthIsland.Protocol.Messages.Player;
using NorthIsland.Protocol.Opcodes;

namespace NorthIsland.Server.Features.Activities;

/// <summary>Trains the life skills: gathering, fishing and alchemy gain experience each time they are used.</summary>
/// <param name="skills">The skill data.</param>
/// <param name="formulas">The formula data, for the experience of each skill level.</param>
/// <param name="options">The activity options, which hold the experience per use.</param>
/// <param name="logger">The logger.</param>
public sealed class LifeSkills(
    SkillData skills,
    FormulaData formulas,
    IOptions<ActivityOptions> options,
    ILogger<LifeSkills> logger)
{
    /// <summary>The fishing skills, best first; the best one the player knows is the one that is trained.</summary>
    private static readonly ushort[] Fishing = [15995, 15994, 15993]; // 15995 Superior Fishing, 15994 Junior Fishing, 15993 Primary Fishing

    /// <summary>Finds the level of the best fishing skill a character knows.</summary>
    /// <param name="character">The character.</param>
    /// <returns>The skill level; 1 for a character who knows no fishing skill.</returns>
    public static int FishingLevel(Character character) => Array.Find(Fishing, id => character.Skill(id) is not null) is var id and not 0 ? character.Skill(id)!.Level : 1;

    /// <summary>Trains the best fishing skill the player knows.</summary>
    /// <param name="session">The player's session.</param>
    public void TrainFishing(GameSession session)
    {
        if (session.ActiveCharacter is { } character && Array.Find(Fishing, id => character.Skill(id) is not null) is var skillId and not 0)
            Train(session, skillId);
    }

    /// <summary>Gives a life skill the experience of one use and tells the client; does nothing when the player does not know the skill.</summary>
    /// <param name="session">The player's session.</param>
    /// <param name="skillId">The life skill.</param>
    public void Train(GameSession session, ushort skillId)
    {
        if (session.ActiveCharacter is not { } character || formulas.Table is not { } formula
            || skills.Train(character, formula, skillId, options.Value.LifeSkillExp) is not { } trained)
            return;
        session.Send(InnMessages.SetAttribute(Action8.AttrSkillExp, trained.Exp, skillId));
        if (!trained.LevelRose)
            return;
        session.Send(InnMessages.SetAttribute(Action8.AttrSkillLevel, trained.Level, skillId));
        logger.LogInformation("[{Session}] {Name}: {Skill} reaches level {Level} ({Exp} skill exp)", session.Id, character.Name, skills[skillId]?.Name,
            trained.Level, trained.Exp);
    }
}
