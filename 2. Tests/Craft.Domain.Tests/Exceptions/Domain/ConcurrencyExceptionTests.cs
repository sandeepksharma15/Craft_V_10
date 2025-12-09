using System.Net;

namespace Craft.Exceptions.Tests.Domain;

public class ConcurrencyExceptionTests
{
    [Fact]
    public void DefaultConstructor_SetsDefaultMessageAndStatusCode()
    {
        // Arrange & Act
        var ex = new ConcurrencyException();

        // Assert
        Assert.Equal("A concurrency conflict occurred. The record has been modified by another user.", ex.Message);
        Assert.Equal(HttpStatusCode.Conflict, ex.StatusCode);
        Assert.Empty(ex.Errors);
    }

    [Fact]
    public void Constructor_WithMessage_SetsMessageAndStatusCode()
    {
        // Arrange & Act
        var ex = new ConcurrencyException("The order has been updated by another user");

        // Assert
        Assert.Equal("The order has been updated by another user", ex.Message);
        Assert.Equal(HttpStatusCode.Conflict, ex.StatusCode);
        Assert.Empty(ex.Errors);
    }

    [Fact]
    public void Constructor_WithMessageAndInnerException_SetsProperties()
    {
        // Arrange
        var inner = new Exception("Database update conflict");

        // Act
        var ex = new ConcurrencyException("Concurrency error occurred", inner);

        // Assert
        Assert.Equal("Concurrency error occurred", ex.Message);
        Assert.Equal(inner, ex.InnerException);
        Assert.Equal(HttpStatusCode.Conflict, ex.StatusCode);
    }

    [Fact]
    public void Constructor_WithMessageAndErrors_SetsAllProperties()
    {
        // Arrange
        var errors = new List<string> { "Row version mismatch", "Record modified at 10:30 AM" };

        // Act
        var ex = new ConcurrencyException("Concurrency conflict detected", errors);

        // Assert
        Assert.Equal("Concurrency conflict detected", ex.Message);
        Assert.Equal(errors, ex.Errors);
        Assert.Equal(2, ex.Errors.Count);
        Assert.Contains("Row version mismatch", ex.Errors);
        Assert.Contains("Record modified at 10:30 AM", ex.Errors);
        Assert.Equal(HttpStatusCode.Conflict, ex.StatusCode);
    }

    [Fact]
    public void Constructor_WithEntityNameAndKey_SetsFormattedMessage()
    {
        // Arrange & Act
        var ex = new ConcurrencyException("Order", 12345);

        // Assert
        Assert.Equal("Concurrency conflict for entity \"Order\" (12345). The record has been modified by another user.", ex.Message);
        Assert.Equal(HttpStatusCode.Conflict, ex.StatusCode);
        Assert.Empty(ex.Errors);
    }

    [Fact]
    public void Constructor_WithEntityNameAndKeyAsString_SetsFormattedMessage()
    {
        // Arrange & Act
        var ex = new ConcurrencyException("Product", "SKU-ABC-123");

        // Assert
        Assert.Equal("Concurrency conflict for entity \"Product\" (SKU-ABC-123). The record has been modified by another user.", ex.Message);
        Assert.Equal(HttpStatusCode.Conflict, ex.StatusCode);
        Assert.Empty(ex.Errors);
    }

    [Fact]
    public void Constructor_WithEntityNameAndKeyAsGuid_SetsFormattedMessage()
    {
        // Arrange
        var guid = Guid.Parse("550e8400-e29b-41d4-a716-446655440000");

        // Act
        var ex = new ConcurrencyException("User", guid);

        // Assert
        Assert.Equal("Concurrency conflict for entity \"User\" (550e8400-e29b-41d4-a716-446655440000). The record has been modified by another user.", ex.Message);
        Assert.Equal(HttpStatusCode.Conflict, ex.StatusCode);
        Assert.Empty(ex.Errors);
    }

    [Fact]
    public void Constructor_WithVersionDetails_SetsFormattedMessageWithVersions()
    {
        // Arrange & Act
        var ex = new ConcurrencyException("Order", 12345, "v1", "v2");

        // Assert
        Assert.Equal("Concurrency conflict for entity \"Order\" (12345). Expected version: v1, Actual version: v2.", ex.Message);
        Assert.Equal(HttpStatusCode.Conflict, ex.StatusCode);
        Assert.Empty(ex.Errors);
    }

    [Fact]
    public void Constructor_WithVersionDetailsAsGuidString_SetsFormattedMessage()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        var expectedVersion = "AAAAAAAADiM=";
        var actualVersion = "AAAAAAAADiQ=";

        // Act
        var ex = new ConcurrencyException("Invoice", entityId, expectedVersion, actualVersion);

        // Assert
        Assert.Contains("Concurrency conflict for entity \"Invoice\"", ex.Message);
        Assert.Contains(entityId.ToString(), ex.Message);
        Assert.Contains($"Expected version: {expectedVersion}", ex.Message);
        Assert.Contains($"Actual version: {actualVersion}", ex.Message);
        Assert.Equal(HttpStatusCode.Conflict, ex.StatusCode);
    }

    [Fact]
    public void Constructor_WithNullErrors_InitializesEmptyErrorsList()
    {
        // Arrange & Act
        var ex = new ConcurrencyException("Concurrency conflict", (List<string>?)null);

        // Assert
        Assert.NotNull(ex.Errors);
        Assert.Empty(ex.Errors);
    }

    [Fact]
    public void Constructor_WithEmptyErrors_InitializesEmptyErrorsList()
    {
        // Arrange
        var errors = new List<string>();

        // Act
        var ex = new ConcurrencyException("Concurrency conflict", errors);

        // Assert
        Assert.NotNull(ex.Errors);
        Assert.Empty(ex.Errors);
    }

    [Fact]
    public void StatusCode_IsAlwaysConflict()
    {
        // Arrange & Act
        var ex1 = new ConcurrencyException();
        var ex2 = new ConcurrencyException("message");
        var ex3 = new ConcurrencyException("Entity", 123);
        var ex4 = new ConcurrencyException("Entity", 123, "v1", "v2");

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, ex1.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, ex2.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, ex3.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, ex4.StatusCode);
    }

    [Fact]
    public void Constructor_WithMultipleErrors_PreservesAllErrors()
    {
        // Arrange
        var errors = new List<string>
        {
            "Expected row version: 0x00000000000007D1",
            "Actual row version: 0x00000000000007D2",
            "Last modified by: john.doe@example.com",
            "Last modified at: 2024-01-15 14:30:00"
        };

        // Act
        var ex = new ConcurrencyException("Detailed concurrency conflict", errors);

        // Assert
        Assert.Equal(4, ex.Errors.Count);
        Assert.All(errors, error => Assert.Contains(error, ex.Errors));
    }

    [Fact]
    public void Constructor_InheritsFromCraftException()
    {
        // Arrange & Act
        var ex = new ConcurrencyException();

        // Assert
        Assert.IsType<CraftException>(ex, exactMatch: false);
        Assert.IsType<Exception>(ex, exactMatch: false);
    }

    [Fact]
    public void Constructor_WithVersionDetails_HandlesNullVersions()
    {
        // Arrange & Act
        var ex = new ConcurrencyException("Order", 123, null!, null!);

        // Assert
        Assert.Contains("Expected version:", ex.Message);
        Assert.Contains("Actual version:", ex.Message);
        Assert.Equal(HttpStatusCode.Conflict, ex.StatusCode);
    }

    [Fact]
    public void Constructor_WithLongEntityName_PreservesFullName()
    {
        // Arrange
        var longEntityName = "VeryLongEntityNameThatExceedsTypicalLengthForTesting";

        // Act
        var ex = new ConcurrencyException(longEntityName, 999);

        // Assert
        Assert.Contains(longEntityName, ex.Message);
        Assert.Equal(HttpStatusCode.Conflict, ex.StatusCode);
    }

    [Fact]
    public void Constructor_WithSpecialCharactersInEntityName_HandlesCorrectly()
    {
        // Arrange & Act
        var ex = new ConcurrencyException("Order<Detail>", 123);

        // Assert
        Assert.Contains("Order<Detail>", ex.Message);
        Assert.Equal(HttpStatusCode.Conflict, ex.StatusCode);
    }
}
