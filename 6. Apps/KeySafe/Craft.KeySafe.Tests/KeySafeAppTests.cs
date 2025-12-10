using Craft.Utilities.Services;
using Moq;

namespace Craft.KeySafe.Tests;

public class KeySafeAppTests
{
    private static readonly byte[] ValidKey = Convert.FromBase64String("REv94eu2m+RaINiX5ETPQDPJ1crSLmWX7YMslPN3Xqg=");
    private static readonly byte[] ValidIV = Convert.FromBase64String("uu1FbeVkYSPn8iK7O9bzzg==");
    private const string PlainText = "Hello, World!";
    private const string EncryptedText = "SomeEncryptedText==";
    private const string DecryptedText = "Hello, World!";

    [Fact]
    public void Generate_Returns_Valid_Key_And_IV()
    {
        var (success, message) = KeySafeApp.Generate();
        Assert.True(success);
        Assert.Contains("Generated Key:", message);
        Assert.Contains("Generated IV:", message);
    }

    [Fact]
    public void Encrypt_Uses_Provided_Key_And_IV()
    {
        Environment.SetEnvironmentVariable("AES_ENCRYPTION_KEY", Convert.ToBase64String(ValidKey));
        Environment.SetEnvironmentVariable("AES_ENCRYPTION_IV", Convert.ToBase64String(ValidIV));

        var app = new KeySafeApp();
        var (success, encrypted) = app.Encrypt(PlainText, ValidKey, ValidIV);
        Assert.True(success);
        Assert.False(string.IsNullOrEmpty(encrypted));
    }

    [Fact]
    public void Decrypt_Uses_Provided_Key_And_IV()
    {
        Environment.SetEnvironmentVariable("AES_ENCRYPTION_KEY", Convert.ToBase64String(ValidKey));
        Environment.SetEnvironmentVariable("AES_ENCRYPTION_IV", Convert.ToBase64String(ValidIV));

        var app = new KeySafeApp();
        var (encSuccess, encrypted) = app.Encrypt(PlainText, ValidKey, ValidIV);
        Assert.True(encSuccess);
        var (decSuccess, decrypted) = app.Decrypt(encrypted, ValidKey, ValidIV);
        Assert.True(decSuccess);
        Assert.Equal(PlainText, decrypted);
    }

    [Fact]
    public void Encrypt_And_Decrypt_With_Service()
    {
        var mock = new Mock<IKeySafeService>();
        mock.Setup(x => x.Encrypt(PlainText)).Returns(EncryptedText);
        mock.Setup(x => x.Decrypt(EncryptedText)).Returns(DecryptedText);
        var app = new KeySafeApp(mock.Object);
        var (encSuccess, encrypted) = app.Encrypt(PlainText);
        Assert.True(encSuccess);
        Assert.Equal(EncryptedText, encrypted);
        var (decSuccess, decrypted) = app.Decrypt(EncryptedText);
        Assert.True(decSuccess);
        Assert.Equal(DecryptedText, decrypted);
    }

    [Fact]
    public void Encrypt_Returns_Error_On_Exception()
    {
        var mock = new Mock<IKeySafeService>();
        mock.Setup(x => x.Encrypt(It.IsAny<string>())).Throws(new Exception("fail"));
        var app = new KeySafeApp(mock.Object);
        var (success, message) = app.Encrypt("fail");
        Assert.False(success);
        Assert.Contains("Encryption error", message);
    }

    [Fact]
    public void Decrypt_Returns_Error_On_Exception()
    {
        var mock = new Mock<IKeySafeService>();
        mock.Setup(x => x.Decrypt(It.IsAny<string>())).Throws(new Exception("fail"));
        var app = new KeySafeApp(mock.Object);
        var (success, message) = app.Decrypt("fail");
        Assert.False(success);
        Assert.Contains("Decryption error", message);
    }

    [Fact]
    public void Encrypt_With_Invalid_Key_Or_IV_Returns_Error()
    {
        Environment.SetEnvironmentVariable("AES_ENCRYPTION_KEY", Convert.ToBase64String(ValidKey));
        Environment.SetEnvironmentVariable("AES_ENCRYPTION_IV", Convert.ToBase64String(ValidIV));

        var app = new KeySafeApp();
        var (success, message) = app.Encrypt(PlainText, new byte[1], new byte[1]);
        Assert.False(success);
        Assert.Contains("Encryption error", message);
    }

    [Fact]
    public void Decrypt_With_Invalid_Key_Or_IV_Returns_Error()
    {
        Environment.SetEnvironmentVariable("AES_ENCRYPTION_KEY", Convert.ToBase64String(ValidKey));
        Environment.SetEnvironmentVariable("AES_ENCRYPTION_IV", Convert.ToBase64String(ValidIV));

        var app = new KeySafeApp();
        var (success, message) = app.Decrypt(EncryptedText, new byte[1], new byte[1]);
        Assert.False(success);
        Assert.Contains("Decryption error", message);
    }

    [Fact]
    public void Encrypt_With_Null_PlainText_Returns_Error()
    {
        Environment.SetEnvironmentVariable("AES_ENCRYPTION_KEY", Convert.ToBase64String(ValidKey));
        Environment.SetEnvironmentVariable("AES_ENCRYPTION_IV", Convert.ToBase64String(ValidIV));

        var app = new KeySafeApp();
        var (success, message) = app.Encrypt(null!, ValidKey, ValidIV);
        Assert.False(success);
        Assert.Contains("Encryption error", message);
    }

    [Fact]
    public void Decrypt_With_Null_CipherText_Returns_Error()
    {
        Environment.SetEnvironmentVariable("AES_ENCRYPTION_KEY", Convert.ToBase64String(ValidKey));
        Environment.SetEnvironmentVariable("AES_ENCRYPTION_IV", Convert.ToBase64String(ValidIV));

        var app = new KeySafeApp();
        var (success, message) = app.Decrypt(null!, ValidKey, ValidIV);
        Assert.False(success);
        Assert.Contains("Decryption error", message);
    }
}
