using System.Net;

namespace Craft.Exceptions.Tests.Domain;

public class GoneExceptionTests
{
    [Fact]
    public void DefaultConstructor_SetsDefaultMessageAndStatusCode()
    {
        // Arrange & Act
        var ex = new GoneException();

        // Assert
        Assert.Equal("The requested resource is no longer available", ex.Message);
        Assert.Equal((HttpStatusCode)410, ex.StatusCode);
        Assert.Empty(ex.Errors);
    }

    [Fact]
    public void Constructor_WithMessage_SetsMessageAndStatusCode()
    {
        // Arrange & Act
        var ex = new GoneException("Resource has been deleted");

        // Assert
        Assert.Equal("Resource has been deleted", ex.Message);
        Assert.Equal((HttpStatusCode)410, ex.StatusCode);
        Assert.Empty(ex.Errors);
    }

    [Fact]
    public void Constructor_WithMessageAndInnerException_SetsProperties()
    {
        // Arrange
        var inner = new Exception("Deletion error");

        // Act
        var ex = new GoneException("Resource gone", inner);

        // Assert
        Assert.Equal("Resource gone", ex.Message);
        Assert.Equal(inner, ex.InnerException);
        Assert.Equal((HttpStatusCode)410, ex.StatusCode);
    }

    [Fact]
    public void Constructor_WithMessageAndErrors_SetsAllProperties()
    {
        // Arrange
        var errors = new List<string> { "Deleted 30 days ago", "Cannot be recovered" };

        // Act
        var ex = new GoneException("Resource permanently deleted", errors);

        // Assert
        Assert.Equal("Resource permanently deleted", ex.Message);
        Assert.Equal(errors, ex.Errors);
        Assert.Equal(2, ex.Errors.Count);
        Assert.Equal((HttpStatusCode)410, ex.StatusCode);
    }

    [Fact]
    public void Constructor_WithEntityNameAndKey_SetsFormattedMessage()
    {
        // Arrange & Act
        var ex = new GoneException("Order", 12345);

        // Assert
        Assert.Equal("Entity \"Order\" (12345) has been permanently deleted and is no longer available.", ex.Message);
        Assert.Equal((HttpStatusCode)410, ex.StatusCode);
        Assert.Empty(ex.Errors);
    }

    [Fact]
    public void Constructor_WithEntityNameKeyAndDeletedAt_SetsFormattedMessage()
    {
        // Arrange
        var deletedAt = new DateTime(2024, 1, 15, 10, 30, 0, DateTimeKind.Utc);

        // Act
        var ex = new GoneException("Product", "SKU-123", deletedAt);

        // Assert
        Assert.Contains("Product", ex.Message);
        Assert.Contains("SKU-123", ex.Message);
        Assert.Contains("2024-01-15 10:30:00", ex.Message);
        Assert.Equal((HttpStatusCode)410, ex.StatusCode);
    }

    [Fact]
    public void Constructor_WithEntityNameAndGuidKey_SetsFormattedMessage()
    {
        // Arrange
        var guid = Guid.Parse("550e8400-e29b-41d4-a716-446655440000");

        // Act
        var ex = new GoneException("User", guid);

        // Assert
        Assert.Contains("User", ex.Message);
        Assert.Contains(guid.ToString(), ex.Message);
        Assert.Contains("permanently deleted", ex.Message);
        Assert.Equal((HttpStatusCode)410, ex.StatusCode);
    }

    [Fact]
    public void Constructor_WithRecentDeletionDate_FormatsCorrectly()
    {
        // Arrange
        var deletedAt = DateTime.UtcNow.AddDays(-7);

        // Act
        var ex = new GoneException("Document", 999, deletedAt);

        // Assert
        Assert.Contains("Document", ex.Message);
        Assert.Contains("999", ex.Message);
        Assert.Contains(deletedAt.ToString("yyyy-MM-dd"), ex.Message);
    }

    [Fact]
    public void Constructor_WithOldDeletionDate_FormatsCorrectly()
    {
        // Arrange
        var deletedAt = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        // Act
        var ex = new GoneException("LegacyOrder", 456, deletedAt);

        // Assert
        Assert.Contains("2020-01-01", ex.Message);
    }

    [Fact]
    public void Constructor_WithNullErrors_InitializesEmptyErrorsList()
    {
        // Arrange & Act
        var ex = new GoneException("Gone", (List<string>?)null);

        // Assert
        Assert.NotNull(ex.Errors);
        Assert.Empty(ex.Errors);
    }

    [Fact]
    public void StatusCode_IsAlways410()
    {
        // Arrange & Act
        var ex1 = new GoneException();
        var ex2 = new GoneException("message");
        var ex3 = new GoneException("Entity", 123);
        var ex4 = new GoneException("Entity", 123, DateTime.UtcNow);

        // Assert
        Assert.Equal((HttpStatusCode)410, ex1.StatusCode);
        Assert.Equal((HttpStatusCode)410, ex2.StatusCode);
        Assert.Equal((HttpStatusCode)410, ex3.StatusCode);
        Assert.Equal((HttpStatusCode)410, ex4.StatusCode);
    }

    [Fact]
    public void Constructor_InheritsFromCraftException()
    {
        // Arrange & Act
        var ex = new GoneException();

        // Assert
        Assert.IsType<CraftException>(ex, exactMatch: false);
        Assert.IsType<Exception>(ex, exactMatch: false);
    }

    [Fact]
    public void Constructor_WithSpecialCharactersInEntityName_HandlesCorrectly()
    {
        // Arrange & Act
        var ex = new GoneException("User-Account", "email@example.com");

        // Assert
        Assert.Contains("User-Account", ex.Message);
        Assert.Contains("email@example.com", ex.Message);
    }
}
