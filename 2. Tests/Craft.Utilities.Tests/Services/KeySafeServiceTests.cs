using System.Text;
using Craft.Utilities.Services;

namespace Craft.Utilities.Tests.Services;

public class KeySafeServiceTests
{
    private readonly KeySafeService _keySafeService;
    private readonly byte[] _validKey = Convert.FromBase64String("REv94eu2m+RaINiX5ETPQDPJ1crSLmWX7YMslPN3Xqg=");
    private readonly byte[] _validIV = Convert.FromBase64String("uu1FbeVkYSPn8iK7O9bzzg==");
    private const string PlainText = "Hello, World!";
    private readonly string _encryptedText;

    public KeySafeServiceTests()
    {
        Environment.SetEnvironmentVariable("AES_ENCRYPTION_KEY", Convert.ToBase64String(_validKey));
        Environment.SetEnvironmentVariable("AES_ENCRYPTION_IV", Convert.ToBase64String(_validIV));

        _keySafeService = new KeySafeService();
        _encryptedText = KeySafeService.Encrypt(PlainText, _validKey, _validIV);
    }

    [Fact]
    public void Constructor_ShouldThrowException_WhenKeyNotFound()
    {
        Environment.SetEnvironmentVariable("AES_ENCRYPTION_KEY", null);
        Assert.Throws<InvalidOperationException>(() => new KeySafeService());
    }

    [Fact]
    public void Constructor_ShouldThrowException_WhenIVNotFound()
    {
        Environment.SetEnvironmentVariable("AES_ENCRYPTION_IV", null);
        Assert.Throws<InvalidOperationException>(() => new KeySafeService());
    }

    [Fact]
    public void Constructor_ShouldThrowException_WhenKeyLengthIsInvalid()
    {
        Environment.SetEnvironmentVariable("AES_ENCRYPTION_KEY", Convert.ToBase64String(Encoding.UTF8.GetBytes("shortkey")));
        Assert.Throws<InvalidOperationException>(() => new KeySafeService());
    }

    [Fact]
    public void Constructor_ShouldThrowException_WhenIVLengthIsInvalid()
    {
        Environment.SetEnvironmentVariable("AES_ENCRYPTION_IV", Convert.ToBase64String(Encoding.UTF8.GetBytes("shortiv")));
        Assert.Throws<InvalidOperationException>(() => new KeySafeService());
    }

    [Fact]
    public void Encrypt_ShouldThrowArgumentException_WhenPlainTextIsNull()
    {
        Assert.Throws<ArgumentException>(() => KeySafeService.Encrypt(null!, _validKey, _validIV));
    }

    [Fact]
    public void Encrypt_ShouldThrowArgumentException_WhenPlainTextIsEmpty()
    {
        Assert.Throws<ArgumentException>(() => KeySafeService.Encrypt(string.Empty, _validKey, _validIV));
    }

    [Fact]
    public void Encrypt_ShouldThrowArgumentException_WhenKeyIsNull()
    {
        Assert.Throws<ArgumentException>(() => KeySafeService.Encrypt(PlainText, null!, _validIV));
    }

    [Fact]
    public void Encrypt_ShouldThrowArgumentException_WhenKeyIsEmpty()
    {
        Assert.Throws<ArgumentException>(() => KeySafeService.Encrypt(PlainText, [], _validIV));
    }

    [Fact]
    public void Encrypt_ShouldThrowArgumentException_WhenIVIsNull()
    {
        Assert.Throws<ArgumentException>(() => KeySafeService.Encrypt(PlainText, _validKey, null!));
    }

    [Fact]
    public void Encrypt_ShouldThrowArgumentException_WhenIVIsEmpty()
    {
        Assert.Throws<ArgumentException>(() => KeySafeService.Encrypt(PlainText, _validKey, []));
    }

    [Fact]
    public void Encrypt_ShouldReturnEncryptedString_WhenInputIsValid()
    {
        var encryptedText = KeySafeService.Encrypt(PlainText, _validKey, _validIV);
        Assert.NotNull(encryptedText);
        Assert.NotEqual(PlainText, encryptedText);
    }

    [Fact]
    public void Decrypt_ShouldThrowArgumentException_WhenCipherTextIsNull()
    {
        Assert.Throws<ArgumentException>(() => KeySafeService.Decrypt(null!, _validKey, _validIV));
    }

    [Fact]
    public void Decrypt_ShouldThrowArgumentException_WhenCipherTextIsEmpty()
    {
        Assert.Throws<ArgumentException>(() => KeySafeService.Decrypt(string.Empty, _validKey, _validIV));
    }

    [Fact]
    public void Decrypt_ShouldThrowArgumentException_WhenKeyIsNull()
    {
        Assert.Throws<ArgumentException>(() => KeySafeService.Decrypt(_encryptedText, null!, _validIV));
    }

    [Fact]
    public void Decrypt_ShouldThrowArgumentException_WhenKeyIsEmpty()
    {
        Assert.Throws<ArgumentException>(() => KeySafeService.Decrypt(_encryptedText, [], _validIV));
    }

    [Fact]
    public void Decrypt_ShouldThrowArgumentException_WhenIVIsNull()
    {
        Assert.Throws<ArgumentException>(() => KeySafeService.Decrypt(_encryptedText, _validKey, null!));
    }

    [Fact]
    public void Decrypt_ShouldThrowArgumentException_WhenIVIsEmpty()
    {
        Assert.Throws<ArgumentException>(() => KeySafeService.Decrypt(_encryptedText, _validKey, []));
    }

    [Fact]
    public void Decrypt_ShouldThrowArgumentException_WhenCipherTextIsNotBase64()
    {
        Assert.Throws<ArgumentException>(() => KeySafeService.Decrypt("InvalidBase64", _validKey, _validIV));
    }

    [Fact]
    public void Decrypt_ShouldThrowInvalidOperationException_WhenDecryptionFails()
    {
        var invalidEncryptedText = Convert.ToBase64String(Encoding.UTF8.GetBytes("InvalidEncryptedText"));
        Assert.Throws<InvalidOperationException>(() => KeySafeService.Decrypt(invalidEncryptedText, _validKey, _validIV));
    }

    [Fact]
    public void Decrypt_ShouldReturnPlainText_WhenInputIsValid()
    {
        var decryptedText = KeySafeService.Decrypt(_encryptedText, _validKey, _validIV);
        Assert.Equal(PlainText, decryptedText);
    }

    [Fact]
    public void GetKey_ShouldReturnKey()
    {
        var key = _keySafeService.GetKey();
        Assert.Equal(Convert.ToBase64String(_validKey), key);
    }

    [Fact]
    public void GetIV_ShouldReturnIV()
    {
        var iv = _keySafeService.GetIV();
        Assert.Equal(Convert.ToBase64String(_validIV), iv);
    }
}
