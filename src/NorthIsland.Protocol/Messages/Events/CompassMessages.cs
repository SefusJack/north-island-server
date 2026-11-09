using NorthIsland.Protocol.IO;
using NorthIsland.Protocol.Opcodes;

namespace NorthIsland.Protocol.Messages.Events;

/// <summary>Builders for the Golden Compass quiz packets (57/10/x).</summary>
public static class CompassMessages
{
    /// <summary>Starts a 57/10 packet with the given sub code.</summary>
    /// <param name="sub">The sub code.</param>
    /// <returns>The writer, positioned after the sub code.</returns>
    private static PacketWriter W(byte sub) => new PacketWriter(Action57.Code, Action57.Compass).U8(sub);

    /// <summary>Builds 57/10/1, which opens the Golden Compass screen.</summary>
    /// <returns>The packet.</returns>
    public static byte[] Open() => W(1).ToArray();

    /// <summary>Builds 57/10/2, which closes the Golden Compass screen.</summary>
    /// <returns>The packet.</returns>
    public static byte[] Close() => W(2).ToArray();

    /// <summary>Builds 57/10/3, a question with four answers.</summary>
    /// <param name="text">The question.</param>
    /// <param name="answers">The answer texts; the first four are sent and missing ones are sent empty.</param>
    /// <returns>The packet.</returns>
    public static byte[] Question(string text, IReadOnlyList<string> answers)
    {
        var w = W(3).PStr8(Clip(text));
        for (var i = 0; i < 4; i++)
            w.PStr8(Clip(i < answers.Count ? answers[i] : ""));
        return w.ToArray();
    }

    /// <summary>Builds 57/10/4, which reveals the correct answer and the lives left.</summary>
    /// <param name="correct">Number of the correct answer.</param>
    /// <param name="lives">Lives the player has left.</param>
    /// <returns>The packet.</returns>
    public static byte[] Reveal(byte correct, byte lives) => W(4).U8(correct).U8(lives).ToArray();

    /// <summary>Builds 57/10/5, the list of winners.</summary>
    /// <param name="names">Player names; at most 20 are sent.</param>
    /// <returns>The packet.</returns>
    public static byte[] Results(IReadOnlyList<string> names)
    {
        var w = W(5).U8((byte)Math.Min(20, names.Count));
        foreach (var name in names.Take(20))
            w.PStr8(name);
        return w.ToArray();
    }

    /// <summary>Builds 57/10/6, which tells the player they are washed out.</summary>
    /// <returns>The packet.</returns>
    public static byte[] WashedOut() => W(6).ToArray();

    /// <summary>Builds 57/10/7, the ids of the players taking part.</summary>
    /// <param name="playerIds">Player ids; at most 20 are sent.</param>
    /// <returns>The packet.</returns>
    public static byte[] Participants(IReadOnlyList<int> playerIds)
    {
        var w = W(7).U8((byte)Math.Min(20, playerIds.Count));
        foreach (var id in playerIds.Take(20))
            w.I32(id);
        return w.ToArray();
    }

    /// <summary>Cuts a text to the 250 characters a packet string can carry.</summary>
    /// <param name="s">The text.</param>
    /// <returns>The text, shortened when needed.</returns>
    private static string Clip(string s) => s.Length > 250 ? s[..250] : s;
}
