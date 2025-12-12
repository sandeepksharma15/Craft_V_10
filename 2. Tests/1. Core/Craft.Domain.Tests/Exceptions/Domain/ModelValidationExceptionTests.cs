using System.Net;

namespace Craft.Domain.Tests.Domain;

public class ModelValidationExceptionTests
{
    [Fact]
    public void DefaultConstructor_SetsDefaultMessageAndStatusCodeAndEmptyValidationErrors()
    {
        // Arrange & Act
        var ex = new ModelValidationException();

        // Assert
        Assert.Equal("One or more validation failures have occurred.", ex.Message);
        Assert.Equal(HttpStatusCode.BadRequest, ex.StatusCode);
        Assert.NotNull(ex.ValidationErrors);
        Assert.Empty(ex.ValidationErrors);
        Assert.Empty(ex.Errors);
    }

    [Fact]
    public void Constructor_WithMessage_SetsMessageAndStatusCode()
    {
        // Arrange & Act
        var ex = new ModelValidationException("custom message");

        // Assert
        Assert.Equal("custom message", ex.Message);
        Assert.Equal(HttpStatusCode.BadRequest, ex.StatusCode);
        Assert.NotNull(ex.ValidationErrors);
        Assert.Empty(ex.ValidationErrors);
        Assert.Empty(ex.Errors);
    }

    [Fact]
    public void Constructor_WithMessageAndInnerException_SetsProperties()
    {
        // Arrange & Act
        var inner = new Exception("inner");
        var ex = new ModelValidationException("msg", inner);

        // Assert
        Assert.Equal("msg", ex.Message);
        Assert.Equal(inner, ex.InnerException);
        Assert.NotNull(ex.ValidationErrors);
        Assert.Empty(ex.ValidationErrors);
        Assert.Empty(ex.Errors);
    }

    [Fact]
    public void Constructor_WithMessageAndValidationErrors_SetsAllPropertiesAndFlattensErrors()
    {
        // Arrange & Act
        var validationErrors = new Dictionary<string, string[]>
        {
            { "Field1", new[] { "Error1", "Error2" } },
            { "Field2", new[] { "Error3" } }
        };
        var ex = new ModelValidationException("msg", validationErrors);

        // Assert
        Assert.Equal("msg", ex.Message);
        Assert.Equal(HttpStatusCode.BadRequest, ex.StatusCode);
        Assert.Equal(validationErrors, ex.ValidationErrors);
        Assert.Contains("Error1", ex.Errors);
        Assert.Contains("Error2", ex.Errors);
        Assert.Contains("Error3", ex.Errors);
        Assert.Equal(3, ex.Errors.Count);
    }
}
