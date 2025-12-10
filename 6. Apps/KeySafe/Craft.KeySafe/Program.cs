using Craft.Extensions.System;

namespace Craft.KeySafe;

public class Program
{
    /// <summary>
    /// Entry point for the KeySafe application.
    /// </summary>
    public static void Main(string[] args)
    {
        if (TryHandleSetKeyOrIv(args))
            return;

        if (!IsValidMainArgs(args))
        {
            PrintUsage();
            return;
        }

        string mode = args[0];

        try
        {
            if (mode == "-g")
            {
                var (success, message) = KeySafeApp.Generate();
                Console.WriteLine(message);
                return;
            }

            var app = new KeySafeApp();

            string? keyString = Environment.GetEnvironmentVariable("AES_ENCRYPTION_KEY");
            string? ivString = Environment.GetEnvironmentVariable("AES_ENCRYPTION_IV");

            byte[]? key = null;
            byte[]? iv = null;

            if (!string.IsNullOrEmpty(keyString) && !string.IsNullOrEmpty(ivString))
            {
                key = Convert.FromBase64String(keyString);
                iv = Convert.FromBase64String(ivString);
            }

            if (args.Length < 2)
            {
                Console.WriteLine("Error: Input text is required for encryption/decryption.");
                return;
            }

            string inputText = args[1];

            if (mode == "-e")
            {
                var (success, message) = app.Encrypt(inputText, key, iv);
                Console.WriteLine(success ? $"Encrypted: {message}" : message);
            }
            else if (mode == "-d")
            {
                var (success, message) = app.Decrypt(inputText, key, iv);
                Console.WriteLine(success ? $"Decrypted: {message}" : message);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Handles --set-key and --set-iv arguments. Returns true if handled.
    /// </summary>
    public static bool TryHandleSetKeyOrIv(string[] args, EnvironmentVariableTarget target = EnvironmentVariableTarget.Process)
    {
        if (args.Length == 2 && args[0] == "--set-key")
        {
            if (!args[1].IsBase64String())
            {
                Console.WriteLine("Error: The provided key is not a valid Base64 string.");
                return true;
            }

            Environment.SetEnvironmentVariable("AES_ENCRYPTION_KEY", args[1], target);

            Console.WriteLine($"{(target == EnvironmentVariableTarget.Machine ? "System-wide " : "Process ")}AES_ENCRYPTION_KEY set successfully.");

            return true;
        }

        if (args.Length == 2 && args[0] == "--set-iv")
        {
            if (!args[1].IsBase64String())
            {
                Console.WriteLine("Error: The provided IV is not a valid Base64 string.");
                return true;
            }

            Environment.SetEnvironmentVariable("AES_ENCRYPTION_IV", args[1], target);
            Console.WriteLine($"{(target == EnvironmentVariableTarget.Machine ? "System-wide " : "Process ")}AES_ENCRYPTION_IV set successfully.");
            return true;
        }

        return false;
    }

    /// <summary>
    /// Checks if the main arguments are valid for encrypt/decrypt/generate.
    /// </summary>
    public static bool IsValidMainArgs(string[] args)
    {
        if (args.Length < 1) return false;
        string[] valid = ["-e", "-d", "-g"];
        return Array.Exists(valid, v => v == args[0]);
    }

    /// <summary>
    /// Prints usage instructions.
    /// </summary>
    public static void PrintUsage()
    {
        Console.WriteLine("Usage:");
        Console.WriteLine("  Encrypt:   KeySafe.exe -e \"Your text here\"");
        Console.WriteLine("  Decrypt:   KeySafe.exe -d \"Your encrypted text here\"");
        Console.WriteLine("  Generate:  KeySafe.exe -g");
        Console.WriteLine("  Set Key:   KeySafe.exe --set-key <base64key>");
        Console.WriteLine("  Set IV:    KeySafe.exe --set-iv <base64iv>");
    }
}
