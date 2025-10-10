using System.Security.Cryptography;
using System.Text;

namespace Craft.Utilities.Passwords;

public static class PasswordGenerator
{
    private const string LowercaseChars = "abcdefghijklmnopqrstuvwxyz";
    private const string NumericChars = "0123456789";
    private const string SpecialChars = "!@#$%^&*()_+[]{}|;:,.<>?";
    private const string UppercaseChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

    /// <summary>
    /// Generates a secure random password containing at least one uppercase, one lowercase, one digit, and one special character.
    /// </summary>
    /// <param name="length">The length of the password. Must be at least 6.</param>
    /// <returns>A randomly generated password string.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if length is less than 6.</exception>
    public static string GeneratePassword(int length = 8)
    {
        if (length < 6)
            throw new ArgumentOutOfRangeException(nameof(length), "Password length must be at least 6.");

        const string pool = UppercaseChars + LowercaseChars + NumericChars + SpecialChars;

        StringBuilder password = new();
        password.Append(GetRandomChar(UppercaseChars));
        password.Append(GetRandomChar(LowercaseChars));
        password.Append(GetRandomChar(NumericChars));
        password.Append(GetRandomChar(SpecialChars));

        for (int i = 4; i < length; i++)
            password.Append(GetRandomChar(pool));

        return ShuffleString(password.ToString());
    }

    private static char GetRandomChar(string charPool)
        => charPool[GetRandomInt(charPool.Length)];

    private static int GetRandomInt(int maxExclusive)
        => RandomNumberGenerator.GetInt32(maxExclusive);

    private static string ShuffleString(string input)
    {
        char[] characters = input.ToCharArray();
        int n = characters.Length;
        while (n > 1)
        {
            n--;
            int k = GetRandomInt(n + 1);
            (characters[n], characters[k]) = (characters[k], characters[n]);
        }

        return new string(characters);
    }
}
