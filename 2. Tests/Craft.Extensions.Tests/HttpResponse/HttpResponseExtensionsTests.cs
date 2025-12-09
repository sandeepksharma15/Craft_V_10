using System.Net;
using System.Text;
using System.Text.Json;
using Craft.Extensions.HttpResponse;

namespace Craft.Extensions.Tests.HttpResponse;

public class HttpResponseExtensionsTests
{
    [Fact]
    public async Task TryReadErrors_ReturnsErrorList_WhenContentIsJsonArray()
    {
        // Arrange
        var errors = new[] { "Error 1", "Error 2" };
        var json = JsonSerializer.Serialize(errors);
        var response = new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await response.TryReadErrors(cancellationToken);

        // Assert
        Assert.Equal(errors, result);
    }

    [Fact]
    public async Task TryReadErrors_ReturnsStringContent_WhenContentIsNotJsonArray()
    {
        // Arrange
        var errorText = "A plain error message.";
        var response = new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent(errorText, Encoding.UTF8, "text/plain")
        };
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await response.TryReadErrors(cancellationToken);

        // Assert
        Assert.Single(result);
        Assert.Equal(errorText, result[0]);
    }

    [Fact]
    public async Task TryReadErrors_ReturnsDefaultError_WhenContentIsEmpty()
    {
        // Arrange
        var response = new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent(string.Empty, Encoding.UTF8, "text/plain"),
            ReasonPhrase = "Not Found"
        };
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await response.TryReadErrors(cancellationToken);

        // Assert
        Assert.Single(result);
        Assert.Equal($"HTTP 404: Not Found", result[0]);
    }

    [Fact]
    public async Task TryReadErrors_ReturnsDefaultError_WhenContentIsWhitespace()
    {
        // Arrange
        var response = new HttpResponseMessage(HttpStatusCode.InternalServerError)
        {
            Content = new StringContent("   ", Encoding.UTF8, "text/plain"),
            ReasonPhrase = "Server Error"
        };
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await response.TryReadErrors(cancellationToken);

        // Assert
        Assert.Single(result);
        Assert.Equal($"HTTP 500: Server Error", result[0]);
    }

    [Fact]
    public async Task TryReadErrors_ReturnsDefaultError_WhenJsonArrayIsNull()
    {
        // Arrange
        var response = new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent("null", Encoding.UTF8, "application/json"),
            ReasonPhrase = "Bad Request"
        };
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await response.TryReadErrors(cancellationToken);

        // Assert
        Assert.Single(result);
        Assert.Equal($"HTTP 400: Bad Request", result[0]);
    }

    [Fact]
    public async Task TryReadErrors_ReturnsDefaultError_WhenContentIsInvalidJson()
    {
        // Arrange
        var response = new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent("{ not valid json }", Encoding.UTF8, "application/json"),
            ReasonPhrase = "Bad Request"
        };
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await response.TryReadErrors(cancellationToken);

        // Assert
        Assert.Single(result);
        Assert.Equal("{ not valid json }", result[0]);
    }
}
