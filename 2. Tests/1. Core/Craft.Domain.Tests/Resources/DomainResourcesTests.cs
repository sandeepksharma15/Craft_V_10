using Craft.Domain.Resources;

namespace Craft.Domain.Tests.Resources;

public class DomainResourcesTests
{
    #region Error Message Resource Tests

    [Fact]
    public void AfterError_ShouldReturnExpectedFormat()
    {
        // Act
        var result = DomainResources.AfterError;

        // Assert
        Assert.Equal("{0} must be after {1}", result);
    }

    [Fact]
    public void LengthError_ShouldReturnExpectedFormat()
    {
        // Act
        var result = DomainResources.LengthError;

        // Assert
        Assert.Equal("{0} cannot be more than {1}", result);
    }

    [Fact]
    public void RequiredError_ShouldReturnExpectedFormat()
    {
        // Act
        var result = DomainResources.RequiredError;

        // Assert
        Assert.Equal("{0} is required", result);
    }

    [Fact]
    public void StartDateGreaterError_ShouldReturnExpectedMessage()
    {
        // Act
        var result = DomainResources.StartDateGreaterError;

        // Assert
        Assert.Equal("Start must be before End", result);
    }

    [Fact]
    public void RangeError_ShouldReturnExpectedFormat()
    {
        // Act
        var result = DomainResources.RangeError;

        // Assert
        Assert.Equal("{0} must be between {1} and {2}", result);
    }

    [Fact]
    public void MinLengthError_ShouldReturnExpectedFormat()
    {
        // Act
        var result = DomainResources.MinLengthError;

        // Assert
        Assert.Equal("{0} must be at least {1} characters", result);
    }

    [Fact]
    public void MaxLengthError_ShouldReturnExpectedFormat()
    {
        // Act
        var result = DomainResources.MaxLengthError;

        // Assert
        Assert.Equal("{0} must not exceed {1} characters", result);
    }

    [Fact]
    public void FormatError_ShouldReturnExpectedFormat()
    {
        // Act
        var result = DomainResources.FormatError;

        // Assert
        Assert.Equal("{0} not in valid format", result);
    }

    [Fact]
    public void DuplicateError_ShouldReturnExpectedMessage()
    {
        // Act
        var result = DomainResources.DuplicateError;

        // Assert
        Assert.Equal("Value should not be same", result);
    }

    #endregion

    #region Exception Message Resource Tests

    [Fact]
    public void ResourceNotFound_ShouldReturnExpectedMessage()
    {
        // Act
        var result = DomainResources.ResourceNotFound;

        // Assert
        Assert.Equal("The requested resource was not found", result);
    }

    [Fact]
    public void EntityNotFound_ShouldReturnExpectedFormat()
    {
        // Act
        var result = DomainResources.EntityNotFound;

        // Assert
        Assert.Contains("{0}", result);
        Assert.Contains("{1}", result);
    }

    [Fact]
    public void EntityAlreadyExists_ShouldReturnExpectedFormat()
    {
        // Act
        var result = DomainResources.EntityAlreadyExists;

        // Assert
        Assert.Contains("{0}", result);
        Assert.Contains("{1}", result);
    }

    [Fact]
    public void ValidationFailed_ShouldReturnExpectedMessage()
    {
        // Act
        var result = DomainResources.ValidationFailed;

        // Assert
        Assert.Equal("One or more validation failures have occurred.", result);
    }

    #endregion

    #region Formatting Helper Method Tests

    [Fact]
    public void FormatAfterError_ShouldFormatCorrectly()
    {
        // Act
        var result = DomainResources.FormatAfterError("EndDate", "StartDate");

        // Assert
        Assert.Equal("EndDate must be after StartDate", result);
    }

    [Fact]
    public void FormatLengthError_ShouldFormatCorrectly()
    {
        // Act
        var result = DomainResources.FormatLengthError("Name", 50);

        // Assert
        Assert.Equal("Name cannot be more than 50", result);
    }

    [Fact]
    public void FormatRequiredError_ShouldFormatCorrectly()
    {
        // Act
        var result = DomainResources.FormatRequiredError("Email");

        // Assert
        Assert.Equal("Email is required", result);
    }

    [Fact]
    public void FormatRangeError_ShouldFormatCorrectly()
    {
        // Act
        var result = DomainResources.FormatRangeError("Age", 18, 65);

        // Assert
        Assert.Equal("Age must be between 18 and 65", result);
    }

    [Fact]
    public void FormatMinLengthError_ShouldFormatCorrectly()
    {
        // Act
        var result = DomainResources.FormatMinLengthError("Password", 8);

        // Assert
        Assert.Equal("Password must be at least 8 characters", result);
    }

    [Fact]
    public void FormatMaxLengthError_ShouldFormatCorrectly()
    {
        // Act
        var result = DomainResources.FormatMaxLengthError("Description", 500);

        // Assert
        Assert.Equal("Description must not exceed 500 characters", result);
    }

    [Fact]
    public void FormatEntityNotFound_ShouldFormatCorrectly()
    {
        // Act
        var result = DomainResources.FormatEntityNotFound("Product", 42);

        // Assert
        Assert.Equal("Entity \"Product\" (42) was not found.", result);
    }

    [Fact]
    public void FormatEntityAlreadyExists_ShouldFormatCorrectly()
    {
        // Act
        var result = DomainResources.FormatEntityAlreadyExists("User", "john@example.com");

        // Assert
        Assert.Equal("Entity \"User\" (john@example.com) already exists.", result);
    }

    [Fact]
    public void FormatConcurrencyConflict_ShouldFormatCorrectly()
    {
        // Act
        var result = DomainResources.FormatConcurrencyConflict("Order", 123);

        // Assert
        Assert.Equal("A concurrency conflict occurred for entity \"Order\" (123).", result);
    }

    [Fact]
    public void FormatConflictError_ShouldFormatCorrectly()
    {
        // Act
        var result = DomainResources.FormatConflictError("Order", "already shipped");

        // Assert
        Assert.Equal("A conflict occurred with resource \"Order\": already shipped", result);
    }

    #endregion

    #region ResourceManager Tests

    [Fact]
    public void ResourceManager_ShouldNotBeNull()
    {
        // Act
        var resourceManager = DomainResources.ResourceManager;

        // Assert
        Assert.NotNull(resourceManager);
    }

    [Fact]
    public void Culture_ShouldBeNullByDefault()
    {
        // Act & Assert
        Assert.Null(DomainResources.Culture);
    }

    #endregion
}
