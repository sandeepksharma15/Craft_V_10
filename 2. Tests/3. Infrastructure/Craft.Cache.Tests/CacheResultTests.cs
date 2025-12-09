using Craft.Cache;

namespace Craft.Cache.Tests;

public class CacheResultTests
{
    #region CacheResult Tests

    [Fact]
    public void Success_CreatesSuccessResult()
    {
        // Arrange & Act
        var result = CacheResult.Success();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Null(result.ErrorMessage);
        Assert.Null(result.Exception);
    }

    [Fact]
    public void Failure_WithMessage_CreatesFailureResult()
    {
        // Arrange & Act
        var result = CacheResult.Failure("Error message");

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Error message", result.ErrorMessage);
        Assert.Null(result.Exception);
    }

    [Fact]
    public void Failure_WithException_StoresException()
    {
        // Arrange
        var exception = new Exception("Test exception");

        // Act
        var result = CacheResult.Failure("Error", exception);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Error", result.ErrorMessage);
        Assert.Same(exception, result.Exception);
    }

    [Fact]
    public void Timestamp_IsSetOnCreation()
    {
        // Arrange
        var before = DateTimeOffset.UtcNow;

        // Act
        var result = CacheResult.Success();
        var after = DateTimeOffset.UtcNow;

        // Assert
        Assert.True(result.Timestamp >= before);
        Assert.True(result.Timestamp <= after);
    }

    #endregion

    #region CacheResult<T> Tests

    [Fact]
    public void GenericSuccess_WithValue_CreatesSuccessResultWithValue()
    {
        // Arrange & Act
        var result = CacheResult<string>.Success("test-value");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.HasValue);
        Assert.Equal("test-value", result.Value);
    }

    [Fact]
    public void GenericSuccess_WithoutValue_CreatesSuccessResultWithoutValue()
    {
        // Arrange & Act
        var result = CacheResult<string>.Success();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(result.HasValue);
        Assert.Null(result.Value);
    }

    [Fact]
    public void GenericSuccess_WithNullValue_CreatesSuccessWithoutValue()
    {
        // Arrange & Act
        var result = CacheResult<string>.Success(null);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(result.HasValue);
        Assert.Null(result.Value);
    }

    [Fact]
    public void GenericFailure_CreatesFailureResult()
    {
        // Arrange & Act
        var result = CacheResult<string>.Failure("Error message");

        // Assert
        Assert.False(result.IsSuccess);
        Assert.False(result.HasValue);
        Assert.Null(result.Value);
        Assert.Equal("Error message", result.ErrorMessage);
    }

    [Fact]
    public void GenericFailure_WithException_StoresException()
    {
        // Arrange
        var exception = new Exception("Test exception");

        // Act
        var result = CacheResult<int>.Failure("Error", exception);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.False(result.HasValue);
        Assert.Equal("Error", result.ErrorMessage);
        Assert.Same(exception, result.Exception);
    }

    [Theory]
    [InlineData(42)]
    [InlineData(0)]
    [InlineData(-1)]
    public void GenericSuccess_WithIntValues_WorksCorrectly(int value)
    {
        // Arrange & Act
        var result = CacheResult<int>.Success(value);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.HasValue);
        Assert.Equal(value, result.Value);
    }

    [Fact]
    public void GenericSuccess_WithComplexType_WorksCorrectly()
    {
        // Arrange
        var testObj = new TestData { Id = 1, Name = "Test" };

        // Act
        var result = CacheResult<TestData>.Success(testObj);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.HasValue);
        Assert.NotNull(result.Value);
        Assert.Equal(1, result.Value.Id);
        Assert.Equal("Test", result.Value.Name);
    }

    #endregion

    private class TestData
    {
        public int Id { get; set; }
        public string? Name { get; set; }
    }
}
