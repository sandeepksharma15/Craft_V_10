using Craft.Utilities.Validators;

namespace Craft.Utilities.Tests.Validators;

public class UrlValidatorsTests
{
    [Theory]
    [InlineData("https://www.example.com", true)]
    [InlineData("http://example.com", true)]
    [InlineData("https://example.com/path/to/page", true)]
    [InlineData("http://localhost:8080", true)]
    [InlineData("https://sub.domain.example.com", true)]
    [InlineData("ftp://example.com", false)]
    [InlineData("not-a-url", false)]
    [InlineData("", false)]
    [InlineData("   ", false)]
    [InlineData("example.com", false)]
    [InlineData("www.example.com", false)]
    [InlineData("javascript:alert('xss')", false)]
    public void IsValidUrl_ValidatesUrlCorrectly(string url, bool expected)
    {
        // Arrange & Act
        var result = UrlValidations.IsValidUrl(url);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void IsValidUrl_WithNull_ReturnsFalse()
    {
        // Arrange & Act
        var result = UrlValidations.IsValidUrl(null!);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task IsUrlReachableAsync_WithValidUrl_ReturnsTrue()
    {
        // Arrange
        var url = "https://www.google.com";

        // Act
        var result = await UrlValidations.IsUrlReachableAsync(url);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task IsUrlReachableAsync_WithInvalidUrl_ReturnsFalse()
    {
        // Arrange
        var url = "https://this-domain-definitely-does-not-exist-12345.com";

        // Act
        var result = await UrlValidations.IsUrlReachableAsync(url);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task IsUrlReachableAsync_WithCancellationToken_ReturnsFalse()
    {
        // Arrange
        var url = "https://www.google.com";
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act
        var result = await UrlValidations.IsUrlReachableAsync(url, cts.Token);

        // Assert
        // The method catches OperationCanceledException and returns false
        Assert.False(result);
    }

    [Fact]
    public async Task IsUrlExistingAsync_WithValidUrl_ReturnsTrue()
    {
        // Arrange
        var url = "https://www.google.com";

        // Act
        var result = await UrlValidations.IsUrlExistingAsync(url);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task IsUrlExistingAsync_WithInvalidUrl_ReturnsFalse()
    {
        // Arrange
        var url = "https://this-domain-definitely-does-not-exist-12345.com";

        // Act
        var result = await UrlValidations.IsUrlExistingAsync(url);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task IsUrlExistingAsync_WithCancellationToken_ReturnsFalse()
    {
        // Arrange
        var url = "https://www.google.com";
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act
        var result = await UrlValidations.IsUrlExistingAsync(url, cts.Token);

        // Assert
        // The method catches OperationCanceledException and returns false
        Assert.False(result);
    }

    [Fact]
    public async Task RemoveInvalidUrls_WithNullList_ReturnsEmptyList()
    {
        // Arrange & Act
        var result = await UrlValidations.RemoveInvalidUrls(null);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task RemoveInvalidUrls_WithEmptyList_ReturnsEmptyList()
    {
        // Arrange
        var urls = new List<string?>();

        // Act
        var result = await UrlValidations.RemoveInvalidUrls(urls);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task RemoveInvalidUrls_WithMixedValidInvalidUrls_ReturnsOnlyValid()
    {
        // Arrange
        var urls = new List<string?>
        {
            "https://www.google.com",
            "not-a-url",
            "https://github.com",
            "ftp://example.com",
            null
        };

        // Act
        var result = await UrlValidations.RemoveInvalidUrls(urls);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Count <= 2);
        Assert.All(result, url => Assert.True(UrlValidations.IsValidUrl(url!)));
    }

    [Fact]
    public async Task RemoveInvalidUrls_WithCancellationToken_HandlesGracefully()
    {
        // Arrange
        var urls = new List<string?> { "https://www.google.com", "https://github.com" };
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act
        var result = await UrlValidations.RemoveInvalidUrls(urls, cts.Token);

        // Assert
        // When cancelled, the HTTP calls fail and return false, so no URLs are considered valid
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Theory]
    [InlineData("https://www.example.com?param=value")]
    [InlineData("https://example.com:8080/path")]
    [InlineData("http://192.168.1.1")]
    [InlineData("https://example.com/path?query=1&other=2#fragment")]
    public void IsValidUrl_WithComplexUrls_ReturnsTrue(string url)
    {
        // Arrange & Act
        var result = UrlValidations.IsValidUrl(url);

        // Assert
        Assert.True(result);
    }

    [Theory]
    [InlineData("mailto:test@example.com")]
    [InlineData("file:///C:/path/to/file")]
    [InlineData("data:text/plain;base64,SGVsbG8=")]
    [InlineData("tel:+1234567890")]
    public void IsValidUrl_WithNonHttpSchemes_ReturnsFalse(string url)
    {
        // Arrange & Act
        var result = UrlValidations.IsValidUrl(url);

        // Assert
        Assert.False(result);
    }
}
