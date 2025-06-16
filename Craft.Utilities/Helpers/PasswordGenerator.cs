using System.Text;

namespace Craft.Utilities.Helpers;

public static class PasswordGenerator
{
    private const string LowercaseChars = "abcdefghijklmnopqrstuvwxyz";
    private const string NumericChars = "0123456789";
    private const string SpecialChars = "!@#$%^&*()_+[]{}|;:,.<>?";
    private const string UppercaseChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private static readonly Random Random = new();

    private static char GetRandomChar(string charPool)
    {
        int index = Random.Next(charPool.Length);
        return charPool[index];
    }

    private static string ShuffleString(string input)
    {
        char[] characters = input.ToCharArray();
        int n = characters.Length;
        while (n > 1)
        {
            n--;
            int k = Random.Next(n + 1);
            (characters[n], characters[k]) = (characters[k], characters[n]);
        }
        return new string(characters);
    }

    public static string GeneratePassword(int length = 8)
    {
        // Create a pool of characters containing at least one from each category
        const string pool = UppercaseChars + LowercaseChars + NumericChars + SpecialChars;

        // Initialize the password with the required characters
        StringBuilder password = new();
        password.Append(GetRandomChar(UppercaseChars));
        password.Append(GetRandomChar(LowercaseChars));
        password.Append(GetRandomChar(NumericChars));
        password.Append(GetRandomChar(SpecialChars));

        // Fill the remaining characters randomly
        for (int i = 4; i < length; i++)
            password.Append(GetRandomChar(pool));

        // Shuffle the characters to make it more random
        return ShuffleString(password.ToString());
    }
}
