namespace NorthIsland.Server.Features.Minigames;

/// <summary>A quiz question with four answers.</summary>
/// <param name="Text">The question text.</param>
/// <param name="Answers">The four answer texts.</param>
/// <param name="Correct">The right answer, 1..4.</param>
public sealed record QuizQuestion(
    string Text,
    string[] Answers,
    byte Correct);
