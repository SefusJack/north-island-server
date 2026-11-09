using System.Collections.Immutable;

namespace NorthIsland.Server.Features.Activities;

/// <summary>Map ids, menu answers and progress marks of the weekly event arenas (quiz, compass, treasure, round challenge) and their lobby.</summary>
public static class EventArenas
{
    /// <summary>The map id of the lobby the sign-up menus are in.</summary>
    public const ushort LobbyMapId = 12065; // 4F, the Capitol

    /// <summary>The warp to the lobby, used to send players back from an arena.</summary>
    public static readonly DoorWarp Lobby = new(LobbyMapId, 305, 246, 0);

    /// <summary>The NPC id of the sign-up staff in the lobby.</summary>
    public const ushort SignUpStaffNpcId = 14217; // Sign-Up Staff

    /// <summary>The menu number of the sign-up staff's menu.</summary>
    public const int SignUpMenu = 4;

    /// <summary>The answer code of the Golden Compass choice in the sign-up menu.</summary>
    public const int CompassChoice = 31;

    /// <summary>The answer code of the Q&amp;A choice in the sign-up menu.</summary>
    public const int QuizChoice = 32;

    /// <summary>The map id of the Q&amp;A hall.</summary>
    public const ushort QuizMapId = 10079; // Quiz Zone

    /// <summary>The map id of the Golden Compass arena.</summary>
    public const ushort CompassMapId = 10095; // Quiz Zone

    /// <summary>The map id of the first National Treasure room.</summary>
    public const ushort TreasureMapId = 10073; // Treasure Room1

    /// <summary>The map id of the first round challenge map.</summary>
    public const ushort ChallengeMapId = 10041; // Challenge Round 1

    /// <summary>The menu number of the Challenge Sign-up menu.</summary>
    public const int ChallengeMenu = 1;

    /// <summary>The answer code of the round challenge choice in <see cref="ChallengeMenu"/>.</summary>
    public const int ChallengeChoice = 30;

    /// <summary>The answer code of the National Treasure choice in the sign-up menu.</summary>
    public const int TreasureChoice = 30;

    /// <summary>The calendar activity key a menu answer signs up for.</summary>
    /// <param name="interact">The menu number the answer was given in.</param>
    /// <param name="code">The answer code.</param>
    /// <returns>The activity key of the <c>EventCalendar</c>, or null when the answer is not a sign-up.</returns>
    public static string? ActivityOfAnswer(int interact, int code) => (interact, code) switch
    {
        (SignUpMenu, CompassChoice) => EventCalendar.ActivityCompass, // compass choice in the sign-up menu: compass activity
        (SignUpMenu, QuizChoice) => EventCalendar.ActivityQuiz, // quiz choice in the sign-up menu: quiz activity
        (SignUpMenu, TreasureChoice) => EventCalendar.ActivityTreasure, // treasure choice in the sign-up menu: treasure activity
        (ChallengeMenu, ChallengeChoice) => EventCalendar.ActivityChallenge, // challenge choice in the challenge menu: challenge activity
        _ => null, // not a sign-up answer: no activity
    };

    /// <summary>The calendar activity key played on an arena's first map.</summary>
    /// <param name="mapId">The map id.</param>
    /// <returns>The activity key, or null when the map is not the first map of an activity.</returns>
    public static string? ActivityOfMap(int mapId) => mapId switch
    {
        QuizMapId => EventCalendar.ActivityQuiz, // first quiz map: quiz activity
        CompassMapId => EventCalendar.ActivityCompass, // first compass map: compass activity
        TreasureMapId => EventCalendar.ActivityTreasure, // first treasure map: treasure activity
        ChallengeMapId => EventCalendar.ActivityChallenge, // first challenge map: challenge activity
        _ => null, // not the first map of an activity: none
    };

    /// <summary>The quest flags that mark the weekly activities as done; cleared by <see cref="WeeklyReset"/>.</summary>
    public static readonly ImmutableArray<ushort> DoneFlags = [50285, 50289]; // 50285 Treasure Battle, 50289 Battle Royale loss

    /// <summary>The week number used for the weekly reset.</summary>
    /// <param name="now">The time to take the week of.</param>
    /// <returns>The ISO year times 100 plus the ISO week of the year.</returns>
    public static int WeekOf(DateTime now) =>
        System.Globalization.ISOWeek.GetYear(now) * 100 + System.Globalization.ISOWeek.GetWeekOfYear(now);

    /// <summary>
    /// Starts a new week for a character: when the stored week differs from the week of <paramref name="now"/> it stores the new week,
    /// clears the done flags and, unless the character stands in an arena, the run marks.
    /// </summary>
    /// <param name="character">The character to reset.</param>
    /// <param name="now">The current time.</param>
    /// <returns>True when a new week began.</returns>
    public static bool WeeklyReset(Character character, DateTime now)
    {
        var week = WeekOf(now);
        if (character.ActivityWeek == week)
            return false;
        character.ActivityWeek = week;
        foreach (var id in DoneFlags)
            character.MarkFlags.Remove(id);
        if (!IsArena(character.MapId))
            ClearRun(character);
        return true;
    }

    /// <summary>The id of the talk line said when an activity is not open.</summary>
    public const ushort NotOpenTalk = 45380; // Sorry, it is not opened at present. Please come later if you have any questions. Thanks for your understanding.

    /// <summary>The id of the talk line telling when an activity is open.</summary>
    /// <param name="activity">The calendar activity key.</param>
    /// <returns>The talk id, or 0 when the activity has none.</returns>
    public static ushort HoursTalk(string activity) => activity == EventCalendar.ActivityQuiz ? (ushort)32581
        : activity == EventCalendar.ActivityCompass ? (ushort)50068 : (ushort)0;

    /// <summary>Tells whether a map belongs to the round challenge.</summary>
    /// <param name="mapId">The map id.</param>
    /// <returns>True for a round challenge map.</returns>
    public static bool IsChallengeMap(int mapId) => mapId is >= 10041 and <= 10052 // 10041 Challenge Round 1, 10052 Challenge Round 12
        or >= 10054 and <= 10056 // 10054 Challenge Round 13, 10056 Challenge Round 15
        or >= 10104 and <= 10108; // 10104 Round 16 Challenge, 10108 Round 20 Challenge

    /// <summary>Tells whether a map belongs to National Treasure.</summary>
    /// <param name="mapId">The map id.</param>
    /// <returns>True for a treasure room.</returns>
    public static bool IsTreasureMap(int mapId) => mapId is >= 10073 and <= 10078; // 10073 Treasure Room1, 10078 Treasure Room6

    /// <summary>Tells whether a map is the Q&amp;A hall or the Golden Compass arena.</summary>
    /// <param name="mapId">The map id.</param>
    /// <returns>True for one of the two.</returns>
    public static bool IsQuizMap(int mapId) => mapId is 10079 or 10095; // 10079 Quiz Zone, 10095 Quiz Zone

    /// <summary>Tells whether a map belongs to any of the event arenas.</summary>
    /// <param name="mapId">The map id.</param>
    /// <returns>True for an arena map.</returns>
    public static bool IsArena(int mapId) => IsChallengeMap(mapId) || IsTreasureMap(mapId) || IsQuizMap(mapId);

    /// <summary>The progress marks of the activity played on a map, which are lost when the player leaves the arena.</summary>
    /// <param name="mapId">The map id.</param>
    /// <returns>The mark ids, empty for a map without progress marks.</returns>
    public static ushort[] ProgressMarks(int mapId) => IsTreasureMap(mapId)
        ? [50284, 50315] // 50284 Treasure Battle, 50315 Signed up for Treasure Battle
        : IsChallengeMap(mapId) ? [50122, 50125] : []; // 50122 King's Team PK, 50125 Signed up for the challenge

    /// <summary>All progress marks of the treasure and challenge runs.</summary>
    public static readonly ImmutableArray<ushort> RunMarks = [50122, // King's Team PK
        50125, // Signed up for the challenge
        50284, // Treasure Battle
        50315]; // Signed up for Treasure Battle

    /// <summary>Removes run marks and flags from a character.</summary>
    /// <param name="character">The character to clear.</param>
    /// <param name="marks">The mark ids to remove; <see cref="RunMarks"/> when null.</param>
    /// <returns>The ids that were present and have been removed.</returns>
    public static List<ushort> ClearRun(Character character, IEnumerable<ushort>? marks = null)
    {
        List<ushort> cleared = [];
        foreach (var id in marks ?? RunMarks)
        {
            var had = character.Marks.RemoveAll(m => m.Id == id) > 0;
            had |= character.MarkFlags.Remove(id);
            if (had)
                cleared.Add(id);
        }
        return cleared;
    }

    /// <summary>Puts a character back in the lobby and clears the progress marks of the arena it stood in.</summary>
    /// <param name="character">The character to send back.</param>
    /// <returns>The mark ids that were removed.</returns>
    public static List<ushort> SendBack(Character character)
    {
        var cleared = ClearRun(character, ProgressMarks(character.MapId));
        (character.MapId, character.X, character.Y) = (Lobby.MapId, Lobby.X, Lobby.Y);
        return cleared;
    }
}
