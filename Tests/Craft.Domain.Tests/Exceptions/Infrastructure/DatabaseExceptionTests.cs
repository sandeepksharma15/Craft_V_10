using System.Net;

namespace Craft.Exceptions.Tests.Infrastructure;

public class DatabaseExceptionTests
{
    [Fact]
    public void DefaultConstructor_SetsDefaultMessageAndStatusCode()
    {
        // Arrange & Act
        var ex = new DatabaseException();

        // Assert
        Assert.Equal("A database error occurred", ex.Message);
        Assert.Equal(HttpStatusCode.InternalServerError, ex.StatusCode);
        Assert.Empty(ex.Errors);
    }

    [Fact]
    public void Constructor_WithMessage_SetsMessageAndStatusCode()
    {
        // Arrange & Act
        var ex = new DatabaseException("Failed to connect to database");

        // Assert
        Assert.Equal("Failed to connect to database", ex.Message);
        Assert.Equal(HttpStatusCode.InternalServerError, ex.StatusCode);
        Assert.Empty(ex.Errors);
    }

    [Fact]
    public void Constructor_WithMessageAndInnerException_SetsProperties()
    {
        // Arrange
        var inner = new Exception("Connection timeout");

        // Act
        var ex = new DatabaseException("Database connection failed", inner);

        // Assert
        Assert.Equal("Database connection failed", ex.Message);
        Assert.Equal(inner, ex.InnerException);
        Assert.Equal(HttpStatusCode.InternalServerError, ex.StatusCode);
    }

    [Fact]
    public void Constructor_WithMessageAndErrors_SetsAllProperties()
    {
        // Arrange
        var errors = new List<string>
        {
            "Connection pool exhausted",
            "Maximum retry attempts exceeded",
            "Network unreachable"
        };

        // Act
        var ex = new DatabaseException("Database operation failed", errors);

        // Assert
        Assert.Equal("Database operation failed", ex.Message);
        Assert.Equal(errors, ex.Errors);
        Assert.Equal(3, ex.Errors.Count);
        Assert.Contains("Connection pool exhausted", ex.Errors);
        Assert.Contains("Maximum retry attempts exceeded", ex.Errors);
        Assert.Contains("Network unreachable", ex.Errors);
        Assert.Equal(HttpStatusCode.InternalServerError, ex.StatusCode);
    }

    [Fact]
    public void Constructor_WithOperationAndDetails_SetsFormattedMessage()
    {
        // Arrange & Act
        var ex = new DatabaseException("INSERT", "Unique constraint violation on column 'Email'");

        // Assert
        Assert.Equal("Database error during INSERT: Unique constraint violation on column 'Email'", ex.Message);
        Assert.Equal(HttpStatusCode.InternalServerError, ex.StatusCode);
        Assert.Empty(ex.Errors);
    }

    [Fact]
    public void Constructor_WithOperationAndDetails_HandlesVariousOperations()
    {
        // Arrange & Act
        var ex1 = new DatabaseException("SELECT", "Table 'Orders' does not exist");
        var ex2 = new DatabaseException("UPDATE", "Deadlock detected");
        var ex3 = new DatabaseException("DELETE", "Foreign key constraint violation");
        var ex4 = new DatabaseException("MIGRATION", "Column type mismatch");

        // Assert
        Assert.Contains("SELECT", ex1.Message);
        Assert.Contains("UPDATE", ex2.Message);
        Assert.Contains("DELETE", ex3.Message);
        Assert.Contains("MIGRATION", ex4.Message);
    }

    [Fact]
    public void Constructor_WithNullErrors_InitializesEmptyErrorsList()
    {
        // Arrange & Act
        var ex = new DatabaseException("Database error", (List<string>?)null);

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
        var ex = new DatabaseException("Database error", errors);

        // Assert
        Assert.NotNull(ex.Errors);
        Assert.Empty(ex.Errors);
    }

    [Fact]
    public void StatusCode_IsAlwaysInternalServerError()
    {
        // Arrange & Act
        var ex1 = new DatabaseException();
        var ex2 = new DatabaseException("message");
        var ex3 = new DatabaseException("operation", "details");

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, ex1.StatusCode);
        Assert.Equal(HttpStatusCode.InternalServerError, ex2.StatusCode);
        Assert.Equal(HttpStatusCode.InternalServerError, ex3.StatusCode);
    }

    [Fact]
    public void Constructor_WithMultipleErrors_PreservesAllErrors()
    {
        // Arrange
        var errors = new List<string>
        {
            "Query timeout after 30 seconds",
            "Connection lost during transaction",
            "Rollback initiated",
            "Data integrity check failed"
        };

        // Act
        var ex = new DatabaseException("Transaction failed", errors);

        // Assert
        Assert.Equal(4, ex.Errors.Count);
        Assert.All(errors, error => Assert.Contains(error, ex.Errors));
    }

    [Fact]
    public void Constructor_InheritsFromCraftException()
    {
        // Arrange & Act
        var ex = new DatabaseException();

        // Assert
        Assert.IsType<CraftException>(ex, exactMatch: false);
        Assert.IsType<Exception>(ex, exactMatch: false);
    }

    [Fact]
    public void Constructor_WithSqlServerException_PreservesInnerException()
    {
        // Arrange
        var inner = new InvalidOperationException("SQL Server error 547: Foreign key constraint violation");

        // Act
        var ex = new DatabaseException("Failed to delete record", inner);

        // Assert
        Assert.Equal(inner, ex.InnerException);
        Assert.Contains("Failed to delete record", ex.Message);
    }

    [Fact]
    public void Constructor_WithLongOperationName_PreservesFullName()
    {
        // Arrange
        var longOperation = "COMPLEX_STORED_PROCEDURE_WITH_MULTIPLE_PARAMETERS_EXECUTION";

        // Act
        var ex = new DatabaseException(longOperation, "Execution timeout");

        // Assert
        Assert.Contains(longOperation, ex.Message);
        Assert.Equal(HttpStatusCode.InternalServerError, ex.StatusCode);
    }

    [Fact]
    public void Constructor_WithDetailedErrorMessage_PreservesFullDetails()
    {
        // Arrange
        var detailedError = "Violation of PRIMARY KEY constraint 'PK_Users'. Cannot insert duplicate key in object 'dbo.Users'. The duplicate key value is (12345).";

        // Act
        var ex = new DatabaseException("INSERT", detailedError);

        // Assert
        Assert.Contains(detailedError, ex.Message);
        Assert.Equal(HttpStatusCode.InternalServerError, ex.StatusCode);
    }

    [Fact]
    public void Constructor_WithConnectionStringError_DoesNotExposeCredentials()
    {
        // Arrange
        var safeError = "Failed to connect to server: authentication failed";

        // Act
        var ex = new DatabaseException("CONNECTION", safeError);

        // Assert
        Assert.Contains(safeError, ex.Message);
        Assert.DoesNotContain("password", ex.Message.ToLower());
        Assert.DoesNotContain("pwd", ex.Message.ToLower());
    }
}
