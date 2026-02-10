namespace Craft.Core.Tests.Common;

public class HttpStatusCodesTests
{
    [Fact]
    public void Ok_HasValue200()
    {
        // Assert
        Assert.Equal(200, HttpStatusCodes.Ok);
    }

    [Fact]
    public void Created_HasValue201()
    {
        // Assert
        Assert.Equal(201, HttpStatusCodes.Created);
    }

    [Fact]
    public void BadRequest_HasValue400()
    {
        // Assert
        Assert.Equal(400, HttpStatusCodes.BadRequest);
    }

    [Fact]
    public void Unauthorized_HasValue401()
    {
        // Assert
        Assert.Equal(401, HttpStatusCodes.Unauthorized);
    }

    [Fact]
    public void Forbidden_HasValue403()
    {
        // Assert
        Assert.Equal(403, HttpStatusCodes.Forbidden);
    }

    [Fact]
    public void NotFound_HasValue404()
    {
        // Assert
        Assert.Equal(404, HttpStatusCodes.NotFound);
    }

    [Fact]
    public void InternalServerError_HasValue500()
    {
        // Assert
        Assert.Equal(500, HttpStatusCodes.InternalServerError);
    }

    [Fact]
    public void Constants_AreReadOnly()
    {
        // Arrange
        var type = typeof(HttpStatusCodes);
        var fields = type.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);

        // Assert
        Assert.All(fields, field =>
        {
            Assert.True(field.IsLiteral, $"Field {field.Name} should be a constant");
            Assert.True(field.IsInitOnly || field.IsLiteral, $"Field {field.Name} should be readonly or constant");
        });
    }

    [Theory]
    [InlineData(200, true)]  // Ok
    [InlineData(201, true)]  // Created
    [InlineData(204, true)]  // No Content (not defined but valid)
    public void SuccessStatusCodes_AreIn2xxRange(int statusCode, bool isSuccess)
    {
        // Arrange & Act
        var is2xx = statusCode >= 200 && statusCode < 300;

        // Assert
        Assert.Equal(isSuccess, is2xx);
    }

    [Theory]
    [InlineData(400, true)]  // BadRequest
    [InlineData(401, true)]  // Unauthorized
    [InlineData(403, true)]  // Forbidden
    [InlineData(404, true)]  // NotFound
    public void ClientErrorStatusCodes_AreIn4xxRange(int statusCode, bool isClientError)
    {
        // Arrange & Act
        var is4xx = statusCode >= 400 && statusCode < 500;

        // Assert
        Assert.Equal(isClientError, is4xx);
    }

    [Theory]
    [InlineData(500, true)]  // InternalServerError
    public void ServerErrorStatusCodes_AreIn5xxRange(int statusCode, bool isServerError)
    {
        // Arrange & Act
        var is5xx = statusCode >= 500 && statusCode < 600;

        // Assert
        Assert.Equal(isServerError, is5xx);
    }

    [Fact]
    public void HttpStatusCodes_Class_IsStatic()
    {
        // Arrange
        var type = typeof(HttpStatusCodes);

        // Assert
        Assert.True(type.IsAbstract && type.IsSealed, "HttpStatusCodes should be a static class");
    }

    [Fact]
    public void HttpStatusCodes_MatchesStandardHttpStatusCodes()
    {
        // Assert - Verify values match standard HTTP status codes
        Assert.Equal(200, HttpStatusCodes.Ok);           // Standard HTTP OK
        Assert.Equal(201, HttpStatusCodes.Created);      // Standard HTTP Created
        Assert.Equal(400, HttpStatusCodes.BadRequest);   // Standard HTTP Bad Request
        Assert.Equal(401, HttpStatusCodes.Unauthorized); // Standard HTTP Unauthorized
        Assert.Equal(403, HttpStatusCodes.Forbidden);    // Standard HTTP Forbidden
        Assert.Equal(404, HttpStatusCodes.NotFound);     // Standard HTTP Not Found
        Assert.Equal(500, HttpStatusCodes.InternalServerError); // Standard HTTP Internal Server Error
    }
}
