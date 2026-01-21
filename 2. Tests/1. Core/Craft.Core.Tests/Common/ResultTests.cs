using Craft.Core;

namespace Craft.Core.Tests.Common;

public class ResultTests
{
    #region Result (Non-Generic) Tests

    [Fact]
    public void Success_CreatesSuccessResult()
    {
        // Arrange & Act
        var result = Result.CreateSuccess();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Null(result.ErrorMessage);
        Assert.Null(result.Errors);
    }

    [Fact]
    public void Failure_WithMessage_CreatesFailureResult()
    {
        // Arrange & Act
        var result = Result.CreateFailure("Operation failed");

        // Assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal("Operation failed", result.ErrorMessage);
        Assert.Null(result.Errors);
    }

    [Fact]
    public void Failure_WithEmptyMessage_CreatesFailureResult()
    {
        // Arrange & Act
        var result = Result.CreateFailure(string.Empty);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal(string.Empty, result.ErrorMessage);
    }

    [Fact]
    public void Failure_WithErrorList_CreatesFailureResult()
    {
        // Arrange
        var errors = new List<string> { "Error 1", "Error 2", "Error 3" };

        // Act
        var result = Result.CreateFailure(errors);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Null(result.ErrorMessage);
        Assert.NotNull(result.Errors);
        Assert.Equal(3, result.Errors.Count);
        Assert.Equal("Error 1", result.Errors[0]);
        Assert.Equal("Error 2", result.Errors[1]);
        Assert.Equal("Error 3", result.Errors[2]);
    }

    [Fact]
    public void Failure_WithEmptyErrorList_CreatesFailureResult()
    {
        // Arrange
        var errors = new List<string>();

        // Act
        var result = Result.CreateFailure(errors);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.NotNull(result.Errors);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void IsFailure_IsInverseOfIsSuccess()
    {
        // Arrange
        var successResult = Result.CreateSuccess();
        var failureResult = Result.CreateFailure("Error");

        // Assert
        Assert.Equal(!successResult.IsSuccess, successResult.IsFailure);
        Assert.Equal(!failureResult.IsSuccess, failureResult.IsFailure);
    }

    #endregion

    #region Result<T> Tests

    [Fact]
    public void GenericSuccess_WithValue_CreatesSuccessResult()
    {
        // Arrange & Act
        var result = Result<string>.CreateSuccess("test-value");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Equal("test-value", result.Value);
        Assert.Null(result.ErrorMessage);
        Assert.Null(result.Errors);
    }

    [Fact]
    public void GenericSuccess_WithNullValue_CreatesSuccessResult()
    {
        // Arrange & Act
        var result = Result<string?>.CreateSuccess(null);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Null(result.Value);
    }

    [Fact]
    public void GenericSuccess_WithComplexType_WorksCorrectly()
    {
        // Arrange
        var testObj = new TestData { Id = 1, Name = "Test" };

        // Act
        var result = Result<TestData>.CreateSuccess(testObj);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(1, result.Value.Id);
        Assert.Equal("Test", result.Value.Name);
        Assert.Same(testObj, result.Value);
    }

    [Theory]
    [InlineData(42)]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(int.MaxValue)]
    [InlineData(int.MinValue)]
    public void GenericSuccess_WithIntValues_WorksCorrectly(int value)
    {
        // Arrange & Act
        var result = Result<int>.CreateSuccess(value);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(value, result.Value);
    }

    [Fact]
    public void GenericFailure_WithMessage_CreatesFailureResult()
    {
        // Arrange & Act
        var result = Result<string>.CreateFailure("Error message");

        // Assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Null(result.Value);
        Assert.Equal("Error message", result.ErrorMessage);
        Assert.Null(result.Errors);
    }

    [Fact]
    public void GenericFailure_WithErrorList_CreatesFailureResult()
    {
        // Arrange
        var errors = new List<string> { "Validation error 1", "Validation error 2" };

        // Act
        var result = Result<int>.CreateFailure(errors);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal(0, result.Value);
        Assert.Null(result.ErrorMessage);
        Assert.NotNull(result.Errors);
        Assert.Equal(2, result.Errors.Count);
    }

    [Fact]
    public void GenericResult_InheritsFromBaseResult()
    {
        // Arrange & Act
        var result = Result<string>.CreateSuccess("test");

        // Assert
        Assert.IsType<Result>(result, exactMatch: false);
    }

    #endregion

    #region Map Tests

    [Fact]
    public void Map_OnSuccessResult_TransformsValue()
    {
        // Arrange
        var result = Result<int>.CreateSuccess(5);

        // Act
        var mappedResult = result.Map(x => x * 2);

        // Assert
        Assert.True(mappedResult.IsSuccess);
        Assert.Equal(10, mappedResult.Value);
    }

    [Fact]
    public void Map_OnSuccessResult_CanChangeType()
    {
        // Arrange
        var result = Result<int>.CreateSuccess(42);

        // Act
        var mappedResult = result.Map(x => x.ToString());

        // Assert
        Assert.True(mappedResult.IsSuccess);
        Assert.Equal("42", mappedResult.Value);
    }

    [Fact]
    public void Map_OnFailureResult_PreservesFailure()
    {
        // Arrange
        var result = Result<int>.CreateFailure("Original error");

        // Act
        var mappedResult = result.Map(x => x * 2);

        // Assert
        Assert.False(mappedResult.IsSuccess);
        Assert.Equal("Original error", mappedResult.ErrorMessage);
        Assert.Equal(0, mappedResult.Value);
    }

    [Fact]
    public void Map_OnSuccessResultWithNullValue_ReturnsFailure()
    {
        // Arrange
        var result = Result<string?>.CreateSuccess(null);

        // Act
        var mappedResult = result.Map(x => x!.Length);

        // Assert
        Assert.False(mappedResult.IsSuccess);
        Assert.Equal("Operation failed", mappedResult.ErrorMessage);
    }

    [Fact]
    public void Map_ChainMultipleMaps_WorksCorrectly()
    {
        // Arrange
        var result = Result<int>.CreateSuccess(5);

        // Act
        var mappedResult = result
            .Map(x => x * 2)
            .Map(x => x + 3)
            .Map(x => x.ToString());

        // Assert
        Assert.True(mappedResult.IsSuccess);
        Assert.Equal("13", mappedResult.Value);
    }

    [Fact]
    public void Map_WithComplexTransformation_WorksCorrectly()
    {
        // Arrange
        var result = Result<TestData>.CreateSuccess(new TestData { Id = 1, Name = "Test" });

        // Act
        var mappedResult = result.Map(x => new TestDto { DisplayName = $"{x.Name} ({x.Id})" });

        // Assert
        Assert.True(mappedResult.IsSuccess);
        Assert.Equal("Test (1)", mappedResult.Value!.DisplayName);
    }

    #endregion

    #region Bind Tests

    [Fact]
    public void Bind_OnSuccessResult_ExecutesBinder()
    {
        // Arrange
        var result = Result<int>.CreateSuccess(5);

        // Act
        var boundResult = result.Bind(x => Result<string>.CreateSuccess(x.ToString()));

        // Assert
        Assert.True(boundResult.IsSuccess);
        Assert.Equal("5", boundResult.Value);
    }

    [Fact]
    public void Bind_OnSuccessResult_CanReturnFailure()
    {
        // Arrange
        var result = Result<int>.CreateSuccess(5);

        // Act
        var boundResult = result.Bind(x =>
            x < 10 ? Result<string>.CreateSuccess(x.ToString()) : Result<string>.CreateFailure("Value too large"));

        // Assert
        Assert.True(boundResult.IsSuccess);
        Assert.Equal("5", boundResult.Value);
    }

    [Fact]
    public void Bind_OnSuccessResultWithFailingBinder_ReturnsFailure()
    {
        // Arrange
        var result = Result<int>.CreateSuccess(15);

        // Act
        var boundResult = result.Bind(x =>
            x < 10 ? Result<string>.CreateSuccess(x.ToString()) : Result<string>.CreateFailure("Value too large"));

        // Assert
        Assert.False(boundResult.IsSuccess);
        Assert.Equal("Value too large", boundResult.ErrorMessage);
    }

    [Fact]
    public void Bind_OnFailureResult_DoesNotExecuteBinder()
    {
        // Arrange
        var result = Result<int>.CreateFailure("Original error");
        var binderExecuted = false;

        // Act
        var boundResult = result.Bind(x =>
        {
            binderExecuted = true;
            return Result<string>.CreateSuccess(x.ToString());
        });

        // Assert
        Assert.False(boundResult.IsSuccess);
        Assert.Equal("Original error", boundResult.ErrorMessage);
        Assert.False(binderExecuted);
    }

    [Fact]
    public void Bind_OnSuccessResultWithNullValue_ReturnsFailure()
    {
        // Arrange
        var result = Result<string?>.CreateSuccess(null);

        // Act
        var boundResult = result.Bind(x => Result<int>.CreateSuccess(x!.Length));

        // Assert
        Assert.False(boundResult.IsSuccess);
        Assert.Equal("Operation failed", boundResult.ErrorMessage);
    }

    [Fact]
    public void Bind_ChainMultipleBinds_WorksCorrectly()
    {
        // Arrange
        var result = Result<int>.CreateSuccess(5);

        // Act
        var boundResult = result
            .Bind(x => Result<int>.CreateSuccess(x * 2))
            .Bind(x => Result<int>.CreateSuccess(x + 3))
            .Bind(x => Result<string>.CreateSuccess(x.ToString()));

        // Assert
        Assert.True(boundResult.IsSuccess);
        Assert.Equal("13", boundResult.Value);
    }

    [Fact]
    public void Bind_ChainWithFailureInMiddle_StopsExecution()
    {
        // Arrange
        var result = Result<int>.CreateSuccess(5);
        var thirdBinderExecuted = false;

        // Act
        var boundResult = result
            .Bind(x => Result<int>.CreateSuccess(x * 2))
            .Bind(x => Result<int>.CreateFailure("Middle error"))
            .Bind(x =>
            {
                thirdBinderExecuted = true;
                return Result<string>.CreateSuccess(x.ToString());
            });

        // Assert
        Assert.False(boundResult.IsSuccess);
        Assert.Equal("Middle error", boundResult.ErrorMessage);
        Assert.False(thirdBinderExecuted);
    }

    [Fact]
    public void Bind_WithComplexBusinessLogic_WorksCorrectly()
    {
        // Arrange
        var result = Result<TestData>.CreateSuccess(new TestData { Id = 1, Name = "Test" });

        // Act
        var boundResult = result.Bind(data =>
        {
            if (string.IsNullOrEmpty(data.Name))
                return Result<TestDto>.CreateFailure("Name is required");

            return Result<TestDto>.CreateSuccess(new TestDto { DisplayName = data.Name });
        });

        // Assert
        Assert.True(boundResult.IsSuccess);
        Assert.Equal("Test", boundResult.Value!.DisplayName);
    }

    #endregion

    #region Combined Map and Bind Tests

    [Fact]
    public void MapAndBind_CanBeCombined()
    {
        // Arrange
        var result = Result<int>.CreateSuccess(5);

        // Act
        var finalResult = result
            .Map(x => x * 2)
            .Bind(x => x < 20 ? Result<string>.CreateSuccess(x.ToString()) : Result<string>.CreateFailure("Too large"))
            .Map(x => $"Result: {x}");

        // Assert
        Assert.True(finalResult.IsSuccess);
        Assert.Equal("Result: 10", finalResult.Value);
    }

    [Fact]
    public void MapAndBind_WithFailure_StopsEarly()
    {
        // Arrange
        var result = Result<int>.CreateSuccess(15);
        var mapExecuted = false;

        // Act
        var finalResult = result
            .Map(x => x * 2)
            .Bind(x => x < 20 ? Result<string>.CreateSuccess(x.ToString()) : Result<string>.CreateFailure("Too large"))
            .Map(x =>
            {
                mapExecuted = true;
                return $"Result: {x}";
            });

        // Assert
        Assert.False(finalResult.IsSuccess);
        Assert.Equal("Too large", finalResult.ErrorMessage);
        Assert.False(mapExecuted);
    }

    #endregion

    #region Edge Cases and Type Safety Tests

    [Fact]
    public void Result_WithDifferentTypes_MaintainsTypeSafety()
    {
        // Arrange & Act
        var stringResult = Result<string>.CreateSuccess("test");
        var intResult = Result<int>.CreateSuccess(42);
        var boolResult = Result<bool>.CreateSuccess(true);

        // Assert
        Assert.IsType<string>(stringResult.Value);
        Assert.IsType<int>(intResult.Value);
        Assert.IsType<bool>(boolResult.Value);
    }

    [Fact]
    public void Result_WithCollectionTypes_WorksCorrectly()
    {
        // Arrange & Act
        var listResult = Result<List<int>>.CreateSuccess([1, 2, 3]);
        var arrayResult = Result<int[]>.CreateSuccess([1, 2, 3]);

        // Assert
        Assert.True(listResult.IsSuccess);
        Assert.Equal(3, listResult.Value!.Count);
        Assert.True(arrayResult.IsSuccess);
        Assert.Equal(3, arrayResult.Value!.Length);
    }

    [Fact]
    public void Result_DefaultValue_IsFailure()
    {
        // Arrange & Act
        var result = default(Result);

        // Assert - default struct will have IsSuccess = false
        Assert.Null(result);
    }

    [Fact]
    public void GenericResult_WithStructType_WorksCorrectly()
    {
        // Arrange & Act
        var result = Result<DateTime>.CreateSuccess(new DateTime(2024, 1, 1));

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(new DateTime(2024, 1, 1), result.Value);
    }

    #endregion

    #region Helper Classes

    private class TestData
    {
        public int Id { get; set; }
        public string? Name { get; set; }
    }

    private class TestDto
    {
        public string? DisplayName { get; set; }
    }

    #endregion
}
