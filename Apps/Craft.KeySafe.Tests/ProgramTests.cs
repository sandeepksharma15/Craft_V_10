using System.Text;

namespace Craft.KeySafe.Tests;

public class ProgramTests
{
    [Theory]
    [ClassData(typeof(InvalidArgsTheoryData))]
    public void Prints_Usage_Or_Error_On_Invalid_Args(string[] args, string expected)
    {
        SetDummyKeyIv();
        var output = CaptureConsoleOutput(() => ProgramMain(args));
        Assert.Contains(expected, output);
    }

    [Fact]
    public void SetKey_InvalidBase64_PrintsError()
    {
        SetDummyKeyIv();
        var output = CaptureConsoleOutput(() => ProgramMain(["--set-key", "notbase64"]));
        Assert.Contains("not a valid Base64 string", output);
    }

    [Fact]
    public void SetIV_InvalidBase64_PrintsError()
    {
        SetDummyKeyIv();
        var output = CaptureConsoleOutput(() => ProgramMain(["--set-iv", "notbase64"]));
        Assert.Contains("not a valid Base64 string", output);
    }

    [Fact]
    public void SetKey_ValidBase64_PrintsSuccess()
    {
        SetDummyKeyIv();
        string base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes("testkeytestkeytestkeytestkeytestkey12"));
        var output = CaptureConsoleOutput(() => ProgramMain(["--set-key", base64]));
        Assert.Contains("Process AES_ENCRYPTION_KEY set successfully", output);
    }

    [Fact]
    public void SetIV_ValidBase64_PrintsSuccess()
    {
        SetDummyKeyIv();
        string base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes("testivtestivtesti"));
        var output = CaptureConsoleOutput(() => ProgramMain(["--set-iv", base64]));
        Assert.Contains("Process AES_ENCRYPTION_IV set successfully", output);
    }

    [Fact]
    public void Generate_PrintsKeyAndIV()
    {
        SetDummyKeyIv();
        var output = CaptureConsoleOutput(() => ProgramMain(["-g"]));
        Assert.Contains("Generated Key:", output);
        Assert.Contains("Generated IV:", output);
    }

    private static void ProgramMain(string[] args)
    {
        var programType = typeof(KeySafeApp).Assembly.GetType("Craft.KeySafe.Program");
        var mainMethod = programType?.GetMethod("Main", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
        mainMethod?.Invoke(null, [args]);
    }

    private static string CaptureConsoleOutput(Action action)
    {
        var sb = new StringBuilder();
        using (var sw = new StringWriter(sb))
        {
            var originalOut = Console.Out;
            Console.SetOut(sw);
            try { action(); }
            finally { Console.SetOut(originalOut); }
        }
        return sb.ToString();
    }

    private static void SetDummyKeyIv()
    {
        Environment.SetEnvironmentVariable("AES_ENCRYPTION_KEY", Convert.ToBase64String(Encoding.UTF8.GetBytes("12345678901234567890123456789012")));
        Environment.SetEnvironmentVariable("AES_ENCRYPTION_IV", Convert.ToBase64String(Encoding.UTF8.GetBytes("1234567890123456")));
    }
}

/// <summary>
/// Provides test data for invalid argument scenarios using xUnit's TheoryData.
/// </summary>
public class InvalidArgsTheoryData : TheoryData<string[], string>
{
    public InvalidArgsTheoryData()
    {
        Add([], "Usage:");
        Add(["-x"], "Usage:");
        Add(["-e"], "Error: Input text is required for encryption/decryption.");
        Add(["-d"], "Error: Input text is required for encryption/decryption.");
    }
}
