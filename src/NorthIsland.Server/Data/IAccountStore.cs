namespace NorthIsland.Server.Data;

/// <summary>Storage of the accounts and their characters.</summary>
public interface IAccountStore
{
    /// <summary>Finds an account by its name, ignoring case.</summary>
    /// <param name="name">The account name.</param>
    /// <param name="ct">Cancels the lookup.</param>
    /// <returns>The account, or null when none has that name.</returns>
    Task<Account?> FindByNameAsync(string name, CancellationToken ct = default);

    /// <summary>Finds an account by its login id.</summary>
    /// <param name="loginId">The account's login id.</param>
    /// <param name="ct">Cancels the lookup.</param>
    /// <returns>The account, or null when none has that id.</returns>
    Task<Account?> FindByLoginIdAsync(int loginId, CancellationToken ct = default);

    /// <summary>Creates and stores a new account with the next free login id.</summary>
    /// <param name="name">The account name; it must not exist yet.</param>
    /// <param name="password">The password; only its hash is stored.</param>
    /// <param name="ct">Cancels the creation.</param>
    /// <returns>The new account.</returns>
    Task<Account> CreateAsync(string name, string password, CancellationToken ct = default);

    /// <summary>Writes the current state of an account to the store.</summary>
    /// <param name="account">The account to save.</param>
    /// <param name="ct">Cancels the save.</param>
    Task SaveAsync(Account account, CancellationToken ct = default);

    /// <summary>Lists all accounts.</summary>
    /// <returns>A snapshot of every account.</returns>
    IReadOnlyList<Account> All();

    /// <summary>Checks whether any account already has a character with the name, ignoring case.</summary>
    /// <param name="characterName">The character name to look for.</param>
    /// <param name="ct">Cancels the check.</param>
    /// <returns>True when the name is taken.</returns>
    Task<bool> IsCharacterNameTakenAsync(string characterName, CancellationToken ct = default);
}
