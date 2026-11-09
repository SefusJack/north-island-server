using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace NorthIsland.Server.Data;

/// <summary>Hashes and verifies account passwords with salted PBKDF2-SHA256.</summary>
public static class PasswordHasher
{
    /// <summary>PBKDF2 iteration count for new hashes.</summary>
    private const int Iterations = 100_000;

    /// <summary>Length of the random salt in bytes.</summary>
    private const int SaltSize = 16;

    /// <summary>Length of the derived hash in bytes.</summary>
    private const int HashSize = 32;

    /// <summary>Hashes a password with a fresh random salt.</summary>
    /// <param name="password">The password in plain text.</param>
    /// <returns>The hash as <c>pbkdf2-sha256$iterations$salt$hash</c> with salt and hash in Base64.</returns>
    public static string Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(Encoding.UTF8.GetBytes(password), salt, Iterations, HashAlgorithmName.SHA256, HashSize);
        return string.Create(CultureInfo.InvariantCulture,
            $"pbkdf2-sha256${Iterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}");
    }

    /// <summary>Checks a password against a stored hash.</summary>
    /// <param name="password">The password in plain text.</param>
    /// <param name="stored">A hash made by <see cref="Hash"/>.</param>
    /// <returns>True when the password matches; false also for a stored value in another format.</returns>
    public static bool Verify(string password, string stored)
    {
        var parts = stored.Split('$');
        if (parts.Length != 4 || parts[0] != "pbkdf2-sha256" ||
            !int.TryParse(parts[1], NumberStyles.None, CultureInfo.InvariantCulture, out var iterations))
            return false;

        var salt = Convert.FromBase64String(parts[2]);
        var expected = Convert.FromBase64String(parts[3]);
        var actual = Rfc2898DeriveBytes.Pbkdf2(Encoding.UTF8.GetBytes(password), salt, iterations, HashAlgorithmName.SHA256, expected.Length);
        return CryptographicOperations.FixedTimeEquals(actual, expected);
    }
}
