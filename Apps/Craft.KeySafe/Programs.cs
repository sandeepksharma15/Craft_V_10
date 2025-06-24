using System.Security.Cryptography;
using Craft.Utilities.Services;

if (args.Length < 1 || (args[0] != "-e" && args[0] != "-d" && args[0] != "-g"))
{
    Console.WriteLine("Usage:");
    Console.WriteLine("  Encrypt:   KeySafe.exe -e \"Your text here\"");
    Console.WriteLine("  Decrypt:   KeySafe.exe -d \"Your encrypted text here\"");
    Console.WriteLine("  Generate:  KeySafe.exe -g");
    return;
}

// Get The Mode
string mode = args[0];

try
{
    if (mode == "-g")
    {
        using var aes = Aes.Create();

        aes.GenerateKey();
        var genKey = aes.Key;

        aes.GenerateIV();
        var genIV = aes.IV;

        Console.WriteLine("Generated Key: " + Convert.ToBase64String(genKey));
        Console.WriteLine("Generated IV: " + Convert.ToBase64String(genIV));

        return;
    }

    string? keyString = Environment.GetEnvironmentVariable("AES_ENCRYPTION_KEY");
    string? ivString = Environment.GetEnvironmentVariable("AES_ENCRYPTION_IV");

    if (string.IsNullOrEmpty(keyString) || string.IsNullOrEmpty(ivString))
    {
        Console.WriteLine("Error: AES_ENCRYPTION_KEY or AES_ENCRYPTION_IV is not set in environment variables.");
        return;
    }

    byte[] key = Convert.FromBase64String(keyString);
    byte[] iv = Convert.FromBase64String(ivString);

    // Get The Input Text
    string inputText = args[1];

    if (mode == "-e")
    {
        string encrypted = KeySafeService.Encrypt(inputText, key, iv);
        Console.WriteLine($"Encrypted: {encrypted}");
    }
    else if (mode == "-d")
    {
        string decrypted = KeySafeService.Decrypt(inputText, key, iv);
        Console.WriteLine($"Decrypted: {decrypted}");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}
