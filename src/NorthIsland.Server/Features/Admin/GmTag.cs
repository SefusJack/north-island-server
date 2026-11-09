namespace NorthIsland.Server.Features.Admin;

/// <summary>Marks the names of GM characters with a prefix.</summary>
public static class GmTag
{
    /// <summary>The text put in front of a GM's name.</summary>
    public const string Prefix = "¢Õ¢Û";

    /// <summary>The name as shown to others: with the prefix when the account is a GM.</summary>
    /// <param name="name">The character name.</param>
    /// <param name="account">The character's account, or null.</param>
    /// <returns>The name to show.</returns>
    public static string Shown(string name, Account? account) => account is { Gm: true } ? Prefix + name : name;

    /// <summary>Removes the prefix from a name, if it has one.</summary>
    /// <param name="name">The name, possibly with the prefix.</param>
    /// <returns>The name without the prefix.</returns>
    public static string Strip(string name) => name.StartsWith(Prefix, StringComparison.Ordinal) ? name[Prefix.Length..] : name;
}
