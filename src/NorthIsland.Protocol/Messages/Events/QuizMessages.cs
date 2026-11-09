using NorthIsland.Protocol.IO;
using NorthIsland.Protocol.Opcodes;

namespace NorthIsland.Protocol.Messages.Events;

/// <summary>Builders for the Q&amp;A quiz packets (57/8/x).</summary>
public static class QuizMessages
{
    /// <summary>Starts a 57/8 packet with the given sub code.</summary>
    /// <param name="sub">The sub code.</param>
    /// <returns>The writer, positioned after the sub code.</returns>
    private static PacketWriter W(byte sub) => new PacketWriter(Action57.Code, Action57.Quiz).U8(sub);

    /// <summary>Builds 57/8/1, a question with four answers.</summary>
    /// <param name="id">Id of the question.</param>
    /// <param name="text">The question.</param>
    /// <param name="answers">The answer texts; the first four are sent and missing ones are sent empty.</param>
    /// <returns>The packet.</returns>
    public static byte[] Question(ushort id, string text, IReadOnlyList<string> answers)
    {
        var w = W(1).U16(id).PStr8(Clip(text));
        for (var i = 0; i < 4; i++)
            w.PStr8(Clip(i < answers.Count ? answers[i] : ""));
        return w.ToArray();
    }

    /// <summary>Builds 57/8/2, the ids of the top players.</summary>
    /// <param name="playerIds">Player ids; at most 20 are sent.</param>
    /// <returns>The packet.</returns>
    public static byte[] Top(IReadOnlyList<int> playerIds)
    {
        var w = W(2).U8((byte)Math.Min(20, playerIds.Count));
        foreach (var id in playerIds.Take(20))
            w.I32(id);
        return w.ToArray();
    }

    /// <summary>Builds 57/8/3, which opens the quiz screen.</summary>
    /// <returns>The packet.</returns>
    public static byte[] Open() => W(3).ToArray();

    /// <summary>Builds 57/8/4, which closes the quiz screen.</summary>
    /// <returns>The packet.</returns>
    public static byte[] Close() => W(4).ToArray();

    /// <summary>Builds 57/8/5, which reveals the correct answer.</summary>
    /// <param name="correct">Number of the correct answer.</param>
    /// <param name="lives">Lives the player has left.</param>
    /// <param name="outcome">Outcome byte for the player.</param>
    /// <returns>The packet.</returns>
    public static byte[] Reveal(byte correct, byte lives, byte outcome) => W(5).U8(correct).U8(lives).U8(outcome).ToArray();

    /// <summary>Builds 57/8/6, which tells the player they are washed out.</summary>
    /// <returns>The packet.</returns>
    public static byte[] WashedOut() => W(6).ToArray();

    /// <summary>Builds 57/8/9, which tells the client the quiz is over.</summary>
    /// <returns>The packet.</returns>
    public static byte[] Over() => W(9).ToArray();

    /// <summary>Builds 57/8/10, the list of winners.</summary>
    /// <param name="names">Player names; at most 20 are sent.</param>
    /// <returns>The packet.</returns>
    public static byte[] Results(IReadOnlyList<string> names)
    {
        var w = W(10).U8((byte)Math.Min(20, names.Count));
        foreach (var name in names.Take(20))
            w.PStr8(name);
        return w.ToArray();
    }

    /// <summary>Builds 57/8/11, which tells the player their answer came too late.</summary>
    /// <returns>The packet.</returns>
    public static byte[] TooLate() => W(11).ToArray();

    /// <summary>Cuts a text to the 250 characters a packet string can carry.</summary>
    /// <param name="s">The text.</param>
    /// <returns>The text, shortened when needed.</returns>
    private static string Clip(string s) => s.Length > 250 ? s[..250] : s;
}
