using System.Globalization;
using NorthIsland.Protocol.Messages.World;

namespace NorthIsland.Server.Features.Activities;

/// <summary>Turns the calendar's weekly activities and running events into the entries of the client's activity list.</summary>
public static class ActivitySchedule
{
    /// <summary>The most entries the list holds; further ones are dropped.</summary>
    public const int MaxEntries = 30;

    /// <summary>The lobby warp the weekly activities teleport to.</summary>
    public static DoorWarp Lobby => EventArenas.Lobby;

    /// <summary>The weekday number used on the wire.</summary>
    /// <param name="day">The day of the week.</param>
    /// <returns>1 for Monday to 7 for Sunday.</returns>
    public static byte Weekday(DayOfWeek day) => day == DayOfWeek.Sunday ? (byte)7 : (byte)day;

    /// <summary>The name shown in the list for an activity key of the calendar.</summary>
    /// <param name="name">The activity key.</param>
    /// <returns>The display name, or the key itself when it is not known.</returns>
    private static string ActivityName(string name) => name.ToLowerInvariant() switch
    {
        EventCalendar.ActivityQuiz => "Q&A", // quiz: shown as Q&A
        EventCalendar.ActivityCompass => "Golden Compass", // compass: shown as Golden Compass
        EventCalendar.ActivityTreasure => "National Treasure", // treasure: shown as National Treasure
        EventCalendar.ActivityChallenge => "Round Challenge", // challenge: shown as Round Challenge
        _ => name, // unknown key: shown as it is
    };

    /// <summary>The description shown in the list for an activity key of the calendar.</summary>
    /// <param name="name">The activity key.</param>
    /// <returns>The description, empty when the key is not known.</returns>
    private static string ActivityDescription(string name) => name.ToLowerInvariant() switch
    {
        EventCalendar.ActivityQuiz => "Answer the host questions in the Q&A hall. Sign up with the Sign-Up Staff in the Challenge lobby.", // quiz rules
        EventCalendar.ActivityCompass => "Find your way on the compass map. Sign up with the Sign-Up Staff in the Challenge lobby.", // compass rules
        EventCalendar.ActivityTreasure => "Open the treasure boxes room by room. Sign up with the Sign-Up Staff in the Challenge lobby.", // treasure rules
        EventCalendar.ActivityChallenge => "Beat the guardian of every round. Sign up at Challenge Sign-up in the Challenge lobby.", // challenge rules
        _ => "", // unknown key: no description
    };

    /// <summary>Cuts a text from the end until it fits a byte limit in Latin-1.</summary>
    /// <param name="text">The text to shorten.</param>
    /// <param name="bytes">The most bytes the text may take.</param>
    /// <returns>The shortened text.</returns>
    private static string Short(string text, int bytes)
    {
        while (System.Text.Encoding.Latin1.GetByteCount(text) > bytes)
            text = text[..^1];
        return text;
    }

    /// <summary>
    /// Builds the list: one entry per weekly activity of the calendar with its state for <paramref name="now"/>
    /// (1 not yet, 2 running, 3 over), then one all-day entry per running event, cut to <see cref="MaxEntries"/>.
    /// </summary>
    /// <param name="file">The calendar file with the weekly activities.</param>
    /// <param name="active">The events running now.</param>
    /// <param name="alwaysOpen">True to show an activity as running during its whole day.</param>
    /// <param name="now">The current time.</param>
    /// <returns>The list entries.</returns>
    public static List<ActivityEntry> Build(CalendarFile file, IReadOnlyList<CalendarEvent> active, bool alwaysOpen, DateTime now)
    {
        List<ActivityEntry> entries = [];
        foreach (var h in file.Activities)
        {
            if (!TimeSpan.TryParseExact(h.Start, @"h\:mm", CultureInfo.InvariantCulture, out var from)
                || !TimeSpan.TryParseExact(h.End, @"h\:mm", CultureInfo.InvariantCulture, out var to) || to <= from)
                continue;
            byte state;
            if (h.Day != now.DayOfWeek)
                state = 1;
            else if (alwaysOpen || (now.TimeOfDay >= from && now.TimeOfDay < to))
                state = 2;
            else
                state = now.TimeOfDay < from ? (byte)1 : (byte)3;
            var lobby = Lobby;
            entries.Add(new ActivityEntry(0, Weekday(h.Day), Short(ActivityName(h.Name), 20), from.TotalDays, (ushort)(to - from).TotalMinutes,
                lobby.MapId, lobby.X, lobby.Y, 0, state, ActivityDescription(h.Name)));
        }
        foreach (var ev in active)
        {
            var parts = new List<string> { $"{ev.Name} is on." };
            if (ev.ExpMultiplier > 1)
                parts.Add($"Battle EXP x{ev.ExpMultiplier.ToString(CultureInfo.InvariantCulture)}.");
            if (ev.Drops.Count > 0)
                parts.Add("Monsters drop extra festival items.");
            entries.Add(new ActivityEntry(0, 0, Short(ev.Name, 20), 0, 1440, 0, 0, 0, 0, 2, Short(string.Join(' ', parts), 254))); // 1440 Minutes, 2 State
        }
        if (entries.Count > MaxEntries)
            entries.RemoveRange(MaxEntries, entries.Count - MaxEntries);
        return entries;
    }

    /// <summary>Computes the version number of a list from its entries.</summary>
    /// <param name="entries">The entries.</param>
    /// <returns>A positive number, never 0.</returns>
    public static int Version(IEnumerable<ActivityEntry> entries)
    {
        var hash = 2166136261u;
        foreach (var e in entries)
        {
            var text = $"{e.Weekday}|{e.Name}|{e.Time:R}|{e.Minutes}|{e.Map}|{e.X}|{e.Y}|{e.MinLevel}|{e.Description}";
            foreach (var b in System.Text.Encoding.Latin1.GetBytes(text))
                hash = unchecked((hash ^ b) * 16777619u);
        }
        var v = (int)(hash & 0x7FFFFFFF);
        return v == 0 ? 1 : v;
    }
}
