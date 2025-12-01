using Craft.Infrastructure.FileUpload;

namespace Craft.Infrastructure.Tests.FileUpload;

public class FileAccessTokenServiceTests
{
    private readonly FileAccessTokenService _tokenService;

    public FileAccessTokenServiceTests()
    {
        _tokenService = new FileAccessTokenService();
    }

    [Fact]
    public void GenerateToken_ReturnsValidToken()
    {
        // Arrange
        var fileId = "file123";
        var expirationMinutes = 60;

        // Act
        var (token, expiresAt) = _tokenService.GenerateToken(fileId, expirationMinutes);

        // Assert
        Assert.NotNull(token);
        Assert.NotEmpty(token);
        Assert.True(expiresAt > DateTimeOffset.UtcNow);
        Assert.True(expiresAt <= DateTimeOffset.UtcNow.AddMinutes(expirationMinutes + 1));
    }

    [Fact]
    public void ValidateToken_ValidToken_ReturnsTrue()
    {
        // Arrange
        var fileId = "file123";
        var (token, _) = _tokenService.GenerateToken(fileId, 60);

        // Act
        var isValid = _tokenService.ValidateToken(token, fileId);

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public void ValidateToken_WrongFileId_ReturnsFalse()
    {
        // Arrange
        var fileId = "file123";
        var wrongFileId = "file456";
        var (token, _) = _tokenService.GenerateToken(fileId, 60);

        // Act
        var isValid = _tokenService.ValidateToken(token, wrongFileId);

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public async Task ValidateToken_ExpiredToken_ReturnsFalse()
    {
        // Arrange
        var fileId = "file123";
        var (token, _) = _tokenService.GenerateToken(fileId, -1);

        await Task.Delay(100);

        // Act
        var isValid = _tokenService.ValidateToken(token, fileId);

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void ValidateToken_InvalidTokenFormat_ReturnsFalse()
    {
        // Arrange
        var invalidToken = "invalid-token-format";
        var fileId = "file123";

        // Act
        var isValid = _tokenService.ValidateToken(invalidToken, fileId);

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void ValidateToken_TamperedToken_ReturnsFalse()
    {
        // Arrange
        var fileId = "file123";
        var (token, _) = _tokenService.GenerateToken(fileId, 60);
        
        var tamperedToken = string.Concat(token.AsSpan(0, token.Length - 5), "XXXXX");

        // Act
        var isValid = _tokenService.ValidateToken(tamperedToken, fileId);

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void GenerateToken_ThrowsArgumentException_ForEmptyFileId()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentException>(() =>
            _tokenService.GenerateToken("", 60));
    }

    [Fact]
    public void ValidateToken_ThrowsArgumentException_ForEmptyToken()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentException>(() =>
            _tokenService.ValidateToken("", "file123"));
    }

    [Fact]
    public void ValidateToken_ThrowsArgumentException_ForEmptyFileId()
    {
        // Arrange
        var (token, _) = _tokenService.GenerateToken("file123", 60);

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            _tokenService.ValidateToken(token, ""));
    }

    [Fact]
    public async Task GenerateToken_DifferentTokensForSameFileId()
    {
        // Arrange
        var fileId = "file123";

        // Act
        var (token1, _) = _tokenService.GenerateToken(fileId, 60);
        await Task.Delay(10);
        var (token2, _) = _tokenService.GenerateToken(fileId, 60);

        // Assert
        Assert.NotEqual(token1, token2);
    }
}
