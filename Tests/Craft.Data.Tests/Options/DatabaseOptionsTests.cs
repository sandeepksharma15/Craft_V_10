using System.ComponentModel.DataAnnotations;

namespace Craft.Data.Tests.Options;

public class DatabaseOptionsTests
{
    [Fact]
    public void Validate_Should_Return_Error_When_DbProvider_Is_Empty()
    {
        // Arrange
        var options = new DatabaseOptions
        {
            DbProvider = string.Empty,
            ConnectionString = "valid connection string"
        };

        // Act
        var results = new List<ValidationResult>();
        var context = new ValidationContext(options);
        var isValid = Validator.TryValidateObject(options, context, results, true);

        // Assert
        Assert.False(isValid);
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(DatabaseOptions.DbProvider)));
    }

    [Fact]
    public void Validate_Should_Return_Error_When_ConnectionString_Is_Empty()
    {
        // Arrange
        var options = new DatabaseOptions
        {
            DbProvider = "mssql",
            ConnectionString = string.Empty
        };

        // Act
        var results = new List<ValidationResult>();
        var context = new ValidationContext(options);
        var isValid = Validator.TryValidateObject(options, context, results, true);

        // Assert
        Assert.False(isValid);
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(DatabaseOptions.ConnectionString)));
    }

    [Fact]
    public void Validate_Should_Return_Multiple_Errors_When_Both_Empty()
    {
        // Arrange
        var options = new DatabaseOptions
        {
            DbProvider = string.Empty,
            ConnectionString = string.Empty
        };

        // Act
        var results = new List<ValidationResult>();
        var context = new ValidationContext(options);
        var isValid = Validator.TryValidateObject(options, context, results, true);

        // Assert
        Assert.False(isValid);
        Assert.Equal(2, results.Count);
    }

    [Fact]
    public void Validate_Should_Pass_When_Required_Properties_Are_Set()
    {
        // Arrange
        var options = new DatabaseOptions
        {
            DbProvider = "mssql",
            ConnectionString = "Server=.;Database=Test;Trusted_Connection=True;"
        };

        // Act
        var results = new List<ValidationResult>();
        var context = new ValidationContext(options);
        var isValid = Validator.TryValidateObject(options, context, results, true);

        // Assert
        Assert.True(isValid);
        Assert.Empty(results);
    }

    [Fact]
    public void Default_Values_Should_Be_Set_Correctly()
    {
        // Arrange & Act
        var options = new DatabaseOptions();

        // Assert
        Assert.Equal(30, options.CommandTimeout);
        Assert.Equal(string.Empty, options.ConnectionString);
        Assert.Equal(string.Empty, options.DbProvider);
        Assert.False(options.EnableDetailedErrors);
        Assert.False(options.EnableSensitiveDataLogging);
        Assert.Equal(3, options.MaxRetryCount);
        Assert.Equal(15, options.MaxRetryDelay);
    }

    [Fact]
    public void SectionName_Should_Be_DatabaseOptions()
    {
        // Assert
        Assert.Equal("DatabaseOptions", DatabaseOptions.SectionName);
    }

    [Fact]
    public void Properties_Can_Be_Set_And_Retrieved()
    {
        // Arrange
        var options = new DatabaseOptions
        {
            CommandTimeout = 60,
            ConnectionString = "Server=localhost;Database=TestDb;",
            DbProvider = "postgresql",
            EnableDetailedErrors = true,
            EnableSensitiveDataLogging = true,
            MaxRetryCount = 5,
            MaxRetryDelay = 30
        };

        // Assert
        Assert.Equal(60, options.CommandTimeout);
        Assert.Equal("Server=localhost;Database=TestDb;", options.ConnectionString);
        Assert.Equal("postgresql", options.DbProvider);
        Assert.True(options.EnableDetailedErrors);
        Assert.True(options.EnableSensitiveDataLogging);
        Assert.Equal(5, options.MaxRetryCount);
        Assert.Equal(30, options.MaxRetryDelay);
    }
}
