using Craft.KeySafe;

class Program
{
    static void Main(string[] args)
    {
        // Check if the user provided the correct number of arguments
        if (args.Length < 1 || (args[0] != "-e" && args[0] != "-d" && args[0] != "-g"))
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("  Encrypt:   KeySafe.exe -e \"Your text here\"");
            Console.WriteLine("  Decrypt:   KeySafe.exe -d \"Your encrypted text here\"");
            Console.WriteLine("  Generate:  KeySafe.exe -g");
            return;
        }

        // Create an instance of the KeySafeApp
        var app = new KeySafeApp();
        string mode = args[0];

        try
        {
            // Handle the different modes: generate, encrypt, decrypt
            if (mode == "-g")
            {
                var (success, message) = KeySafeApp.Generate();
                Console.WriteLine(message);
                return;
            }

            // Retrieve the AES key and IV from environment variables
            string? keyString = Environment.GetEnvironmentVariable("AES_ENCRYPTION_KEY");
            string? ivString = Environment.GetEnvironmentVariable("AES_ENCRYPTION_IV");

            byte[]? key = null;
            byte[]? iv = null;

            // If the key and IV are provided, convert them from Base64 strings
            if (!string.IsNullOrEmpty(keyString) && !string.IsNullOrEmpty(ivString))
            {
                key = Convert.FromBase64String(keyString);
                iv = Convert.FromBase64String(ivString);
            }

            // If the key or IV is not provided, inform the user
            if (args.Length < 2)
            {
                Console.WriteLine("Error: Input text is required for encryption/decryption.");
                return;
            }

            // Retrieve the input text from the command line arguments
            string inputText = args[1];

            // Perform encryption or decryption based on the mode
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
}
