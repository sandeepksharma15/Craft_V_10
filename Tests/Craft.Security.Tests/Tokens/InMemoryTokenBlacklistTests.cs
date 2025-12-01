using Craft.Security.Tokens;
using Microsoft.Extensions.Logging;
using Moq;

namespace Craft.Security.Tests.Tokens;

public class InMemoryTokenBlacklistTests
{
    private readonly Mock<ILogger<InMemoryTokenBlacklist>> _mockLogger;
    private readonly InMemoryTokenBlacklist _blacklist;

    public InMemoryTokenBlacklistTests()
    {
        _mockLogger = new Mock<ILogger<InMemoryTokenBlacklist>>();
        _blacklist = new InMemoryTokenBlacklist(_mockLogger.Object);
    }

    [Fact]
    public async Task AddAsync_Should_AddTokenToBlacklist()
    {
        // Arrange
        var token = "test.token.value";
        var expiration = DateTime.UtcNow.AddHours(1);

        // Act
        await _blacklist.AddAsync(token, expiration);
        var isBlacklisted = await _blacklist.IsBlacklistedAsync(token);

        // Assert
        Assert.True(isBlacklisted);
    }

    [Fact]
    public async Task AddAsync_Should_ThrowException_ForNullToken()
    {
        // Arrange
        var expiration = DateTime.UtcNow.AddHours(1);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _blacklist.AddAsync(null!, expiration));
    }

    [Fact]
    public async Task AddAsync_Should_ThrowException_ForEmptyToken()
    {
        // Arrange
        var expiration = DateTime.UtcNow.AddHours(1);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _blacklist.AddAsync(string.Empty, expiration));
    }

    [Fact]
    public async Task AddAsync_Should_NotThrow_ForDuplicateToken()
    {
        // Arrange
        var token = "test.token.value";
        var expiration = DateTime.UtcNow.AddHours(1);

        // Act
        await _blacklist.AddAsync(token, expiration);
        await _blacklist.AddAsync(token, expiration);

        // Assert
        var isBlacklisted = await _blacklist.IsBlacklistedAsync(token);
        Assert.True(isBlacklisted);
    }

    [Fact]
    public async Task IsBlacklistedAsync_Should_Return_False_ForNonBlacklistedToken()
    {
        // Arrange
        var token = "test.token.value";

        // Act
        var isBlacklisted = await _blacklist.IsBlacklistedAsync(token);

        // Assert
        Assert.False(isBlacklisted);
    }

    [Fact]
    public async Task IsBlacklistedAsync_Should_Return_False_ForNullToken()
    {
        // Act
        var isBlacklisted = await _blacklist.IsBlacklistedAsync(null!);

        // Assert
        Assert.False(isBlacklisted);
    }

    [Fact]
    public async Task IsBlacklistedAsync_Should_Return_True_ForBlacklistedToken()
    {
        // Arrange
        var token = "test.token.value";
        var expiration = DateTime.UtcNow.AddHours(1);
        await _blacklist.AddAsync(token, expiration);

        // Act
        var isBlacklisted = await _blacklist.IsBlacklistedAsync(token);

        // Assert
        Assert.True(isBlacklisted);
    }

    [Fact]
    public async Task CleanupExpiredTokensAsync_Should_RemoveExpiredTokens()
    {
        // Arrange
        var token1 = "expired.token.1";
        var token2 = "valid.token.2";
        var token3 = "expired.token.3";

        var pastExpiration = DateTime.UtcNow.AddHours(-1);
        var futureExpiration = DateTime.UtcNow.AddHours(1);

        await _blacklist.AddAsync(token1, pastExpiration);
        await _blacklist.AddAsync(token2, futureExpiration);
        await _blacklist.AddAsync(token3, pastExpiration);

        // Act
        var removedCount = await _blacklist.CleanupExpiredTokensAsync();

        // Assert
        Assert.Equal(2, removedCount);
        Assert.False(await _blacklist.IsBlacklistedAsync(token1));
        Assert.True(await _blacklist.IsBlacklistedAsync(token2));
        Assert.False(await _blacklist.IsBlacklistedAsync(token3));
    }

    [Fact]
    public async Task CleanupExpiredTokensAsync_Should_Return_Zero_WhenNoExpiredTokens()
    {
        // Arrange
        var token = "valid.token";
        var futureExpiration = DateTime.UtcNow.AddHours(1);
        await _blacklist.AddAsync(token, futureExpiration);

        // Act
        var removedCount = await _blacklist.CleanupExpiredTokensAsync();

        // Assert
        Assert.Equal(0, removedCount);
        Assert.True(await _blacklist.IsBlacklistedAsync(token));
    }

    [Fact]
    public async Task CleanupExpiredTokensAsync_Should_Return_Zero_WhenBlacklistIsEmpty()
    {
        // Act
        var removedCount = await _blacklist.CleanupExpiredTokensAsync();

        // Assert
        Assert.Equal(0, removedCount);
    }

    [Fact]
    public async Task Blacklist_Should_HandleMultipleTokensIndependently()
    {
        // Arrange
        var token1 = "token.1";
        var token2 = "token.2";
        var token3 = "token.3";
        var expiration = DateTime.UtcNow.AddHours(1);

        // Act
        await _blacklist.AddAsync(token1, expiration);
        await _blacklist.AddAsync(token2, expiration);

        // Assert
        Assert.True(await _blacklist.IsBlacklistedAsync(token1));
        Assert.True(await _blacklist.IsBlacklistedAsync(token2));
        Assert.False(await _blacklist.IsBlacklistedAsync(token3));
    }

    [Fact]
    public async Task Blacklist_Should_BeThreadSafe()
    {
        // Arrange
        var tasks = new List<Task>();
        var tokenCount = 100;

        // Act
        for (int i = 0; i < tokenCount; i++)
        {
            var token = $"token.{i}";
            var expiration = DateTime.UtcNow.AddHours(1);
            tasks.Add(Task.Run(async () => await _blacklist.AddAsync(token, expiration)));
        }

        await Task.WhenAll(tasks);

        // Assert
        for (int i = 0; i < tokenCount; i++)
        {
            var token = $"token.{i}";
            Assert.True(await _blacklist.IsBlacklistedAsync(token));
        }
    }
}
