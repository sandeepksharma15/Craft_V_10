using Craft.Core;
using Craft.Core.Common;
using Craft.Core.Common.Constants;

namespace Craft.Core.Tests.Common;

public class ServiceResultExtensionsTests
{
    #region ToServiceResult Tests

    [Fact]
    public void ToServiceResult_ConvertsException_ToFailedServiceResult()
    {
        // Arrange
        var exception = new InvalidOperationException("Test error");

        // Act
        var result = exception.ToServiceResult<string>();

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Test error", result.ErrorMessage);
        Assert.Equal(500, result.StatusCode);
        Assert.Null(result.Value);
    }

    [Fact]
    public void ToServiceResult_WithCustomStatusCode_SetsStatusCode()
    {
        // Arrange
        var exception = new ArgumentException("Invalid argument");

        // Act
        var result = exception.ToServiceResult<int>(400);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(400, result.StatusCode);
        Assert.Equal("Invalid argument", result.ErrorMessage);
    }

    [Fact]
    public void ToServiceResult_WithNullStatusCode_UsesDefault500()
    {
        // Arrange
        var exception = new Exception("Generic error");

        // Act
        var result = exception.ToServiceResult<object>();

        // Assert
        Assert.Equal(500, result.StatusCode);
    }

    #endregion

    #region Map Tests

    [Fact]
    public void Map_OnSuccessResult_TransformsData()
    {
        // Arrange
        var result = ServiceResult<int>.Success(5);

        // Act
        var mapped = result.Map(x => x * 2);

        // Assert
        Assert.True(mapped.IsSuccess);
        Assert.Equal(10, mapped.Value);
    }

    [Fact]
    public void Map_OnSuccessResult_CanChangeType()
    {
        // Arrange
        var result = ServiceResult<int>.Success(42);

        // Act
        var mapped = result.Map(x => x.ToString());

        // Assert
        Assert.True(mapped.IsSuccess);
        Assert.Equal("42", mapped.Value);
    }

    [Fact]
    public void Map_OnFailureResult_PreservesFailure()
    {
        // Arrange
        var errors = new List<string> { "Error1", "Error2" };
        var result = ServiceResult<int>.Failure(errors, ErrorType.NotFound, 404);

        // Act
        var mapped = result.Map(x => x * 2);

        // Assert
        Assert.False(mapped.IsSuccess);
        Assert.Equal(404, mapped.StatusCode);
        Assert.Equal(default, mapped.Value);
    }

    [Fact]
    public void Map_WithNullValue_ReturnsFailure()
    {
        // Arrange
        var result = ServiceResult<string>.Failure("No value");

        // Act
        var mapped = result.Map(x => x.Length);

        // Assert
        Assert.False(mapped.IsSuccess);
    }

    #endregion

    #region Bind Tests

    [Fact]
    public void Bind_OnSuccessResult_CallsBinder()
    {
        // Arrange
        var result = ServiceResult<int>.Success(5);

        // Act
        var bound = result.Bind(x => ServiceResult<string>.Success($"Value: {x}"));

        // Assert
        Assert.True(bound.IsSuccess);
        Assert.Equal("Value: 5", bound.Value);
    }

    [Fact]
    public void Bind_OnFailureResult_PropagatesFailure()
    {
        // Arrange
        var result = ServiceResult<int>.Failure("Original error", ErrorType.Validation, 400);

        // Act
        var bound = result.Bind(x => ServiceResult<string>.Success($"Value: {x}"));

        // Assert
        Assert.False(bound.IsSuccess);
        Assert.Equal("Original error", bound.ErrorMessage);
    }

    #endregion

    #region FirstError Tests

    [Fact]
    public void FirstError_ReturnsFirstError_WhenErrorsExist()
    {
        // Arrange
        var result = ServiceResult<string>.Failure(["Error1", "Error2"]);

        // Act
        var firstError = result.FirstError();

        // Assert
        Assert.Equal("Error1", firstError);
    }

    [Fact]
    public void FirstError_ReturnsNull_WhenNoErrors()
    {
        // Arrange
        var result = ServiceResult<string>.Success("test");

        // Act
        var firstError = result.FirstError();

        // Assert
        Assert.Null(firstError);
    }

    [Fact]
    public void FirstError_WorksWithServiceResult_Type()
    {
        // Arrange
        var result = ServiceResult.Failure("Single error");

        // Act
        var firstError = result.FirstError();

        // Assert
        // ServiceResult stores single error in ErrorMessage, not in Errors list by default
        Assert.Null(firstError); // Because Errors list is null
    }

    [Fact]
    public void FirstError_WorksWithServiceResult_WithErrorList()
    {
        // Arrange
        var errors = new List<string> { "Error A", "Error B" };
        var result = ServiceResult<int>.Failure(errors);

        // Act
        var firstError = result.FirstError();

        // Assert
        Assert.Equal("Error A", firstError);
    }

    [Fact]
    public void FirstError_WorksWithServerResponse()
    {
        // Arrange
        var response = new ServerResponse
        {
            Errors = ["Server error 1", "Server error 2"]
        };

        // Act
        var firstError = response.FirstError();

        // Assert
        Assert.Equal("Server error 1", firstError);
    }

    #endregion

    #region FirstErrorOrDefault Tests

    [Fact]
    public void FirstErrorOrDefault_ReturnsFirstError_WhenErrorsExist()
    {
        // Arrange
        var result = ServiceResult<string>.Failure(["First error", "Second error"]);

        // Act
        var firstError = result.FirstErrorOrDefault();

        // Assert
        Assert.Equal("First error", firstError);
    }

    [Fact]
    public void FirstErrorOrDefault_ReturnsDefault_WhenNoErrors()
    {
        // Arrange
        var result = ServiceResult<string>.Success("test");

        // Act
        var firstError = result.FirstErrorOrDefault("Default message");

        // Assert
        Assert.Equal("Default message", firstError);
    }

    #endregion

    #region CombineErrors Tests

    [Fact]
    public void CombineErrors_CombinesAllErrors_FromMultipleResults()
    {
        // Arrange
        var results = new IServiceResult[]
        {
            ServiceResult<int>.Failure(["Error 1", "Error 2"]),
            ServiceResult<string>.Failure(["Error 3"]),
            ServiceResult<bool>.Success(true)
        };

        // Act
        var combined = results.CombineErrors();

        // Assert
        Assert.Equal(3, combined.Count);
        Assert.Contains("Error 1", combined);
        Assert.Contains("Error 2", combined);
        Assert.Contains("Error 3", combined);
    }

    [Fact]
    public void CombineErrors_RemovesDuplicates()
    {
        // Arrange
        var results = new IServiceResult[]
        {
            ServiceResult<int>.Failure(["Duplicate error"]),
            ServiceResult<string>.Failure(["Duplicate error"])
        };

        // Act
        var combined = results.CombineErrors();

        // Assert
        Assert.Single(combined);
        Assert.Equal("Duplicate error", combined[0]);
    }

    #endregion

    #region AllSuccessful Tests

    [Fact]
    public void AllSuccessful_ReturnsTrue_WhenAllSucceed()
    {
        // Arrange
        var results = new IServiceResult[]
        {
            ServiceResult.Success(),
            ServiceResult<int>.Success(42),
            ServiceResult<string>.Success("test")
        };

        // Act
        var allSuccessful = results.AllSuccessful();

        // Assert
        Assert.True(allSuccessful);
    }

    [Fact]
    public void AllSuccessful_ReturnsFalse_WhenAnyFails()
    {
        // Arrange
        var results = new IServiceResult[]
        {
            ServiceResult.Success(),
            ServiceResult<int>.Failure("Failed"),
            ServiceResult<string>.Success("test")
        };

        // Act
        var allSuccessful = results.AllSuccessful();

        // Assert
        Assert.False(allSuccessful);
    }

    #endregion

    #region GetValueOrDefault Tests

    [Fact]
    public void GetValueOrDefault_ReturnsValue_WhenSuccess()
    {
        // Arrange
        var result = ServiceResult<int>.Success(42);

        // Act
        var value = result.GetValueOrDefault(0);

        // Assert
        Assert.Equal(42, value);
    }

    [Fact]
    public void GetValueOrDefault_ReturnsDefault_WhenFailure()
    {
        // Arrange
        var result = ServiceResult<int>.Failure("Error");

        // Act
        var value = result.GetValueOrDefault(99);

        // Assert
        Assert.Equal(99, value);
    }

    #endregion

    #region GetValueOrThrow Tests

    [Fact]
    public void GetValueOrThrow_ReturnsValue_WhenSuccess()
    {
        // Arrange
        var result = ServiceResult<string>.Success("test value");

        // Act
        var value = result.GetValueOrThrow();

        // Assert
        Assert.Equal("test value", value);
    }

    [Fact]
    public void GetValueOrThrow_ThrowsException_WhenFailure()
    {
        // Arrange
        var result = ServiceResult<string>.Failure("Operation failed");

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => result.GetValueOrThrow());
        Assert.Equal("Operation failed", exception.Message);
    }

    #endregion

    #region Match Tests

    [Fact]
    public void Match_CallsOnSuccess_WhenSuccessful()
    {
        // Arrange
        var result = ServiceResult<int>.Success(10);

        // Act
        var matched = result.Match(
            value => $"Success: {value}",
            _ => "Failure");

        // Assert
        Assert.Equal("Success: 10", matched);
    }

    [Fact]
    public void Match_CallsOnFailure_WhenFailed()
    {
        // Arrange
        var result = ServiceResult<int>.Failure("Error occurred");

        // Act
        var matched = result.Match(
            value => $"Success: {value}",
            r => $"Failure: {r.Message}");

        // Assert
        Assert.Equal("Failure: Error occurred", matched);
    }

    #endregion

    #region Tap Tests

    [Fact]
    public void Tap_ExecutesAction_WhenSuccess()
    {
        // Arrange
        var result = ServiceResult<int>.Success(42);
        var actionExecuted = false;

        // Act
        var tappedResult = result.Tap(_ => actionExecuted = true);

        // Assert
        Assert.True(actionExecuted);
        Assert.Same(result, tappedResult);
    }

    [Fact]
    public void Tap_DoesNotExecuteAction_WhenFailure()
    {
        // Arrange
        var result = ServiceResult<int>.Failure("Error");
        var actionExecuted = false;

        // Act
        var tappedResult = result.Tap(_ => actionExecuted = true);

        // Assert
        Assert.False(actionExecuted);
        Assert.Same(result, tappedResult);
    }

    #endregion

    #region TapError Tests

    [Fact]
    public void TapError_ExecutesAction_WhenFailure()
    {
        // Arrange
        var result = ServiceResult<int>.Failure("Error");
        var actionExecuted = false;

        // Act
        var tappedResult = result.TapError(_ => actionExecuted = true);

        // Assert
        Assert.True(actionExecuted);
        Assert.Same(result, tappedResult);
    }

    [Fact]
    public void TapError_DoesNotExecuteAction_WhenSuccess()
    {
        // Arrange
        var result = ServiceResult<int>.Success(42);
        var actionExecuted = false;

        // Act
        var tappedResult = result.TapError(_ => actionExecuted = true);

        // Assert
        Assert.False(actionExecuted);
        Assert.Same(result, tappedResult);
    }

    #endregion

    #region GetMostSevereErrorType Tests

    [Fact]
    public void GetMostSevereErrorType_ReturnsMostSevere()
    {
        // Arrange
        var results = new IServiceResult[]
        {
            ServiceResult.Failure("Error", ErrorType.Validation),
            ServiceResult<int>.Failure("Error", ErrorType.Internal),
            ServiceResult<string>.Failure("Error", ErrorType.NotFound)
        };

        // Act
        var mostSevere = results.GetMostSevereErrorType();

        // Assert
        Assert.Equal(ErrorType.Internal, mostSevere);
    }

    [Fact]
    public void GetMostSevereErrorType_ReturnsNone_WhenAllSuccessful()
    {
        // Arrange
        var results = new IServiceResult[]
        {
            ServiceResult.Success(),
            ServiceResult<int>.Success(42)
        };

        // Act
        var mostSevere = results.GetMostSevereErrorType();

        // Assert
        Assert.Equal(ErrorType.None, mostSevere);
    }

    #endregion

    #region Async Extension Tests

    [Fact]
    public async Task MapAsync_TransformsValue_WhenSuccess()
    {
        // Arrange
        var resultTask = Task.FromResult(ServiceResult<int>.Success(5));

        // Act
        var mapped = await resultTask.MapAsync(x => x * 2);

        // Assert
        Assert.True(mapped.IsSuccess);
        Assert.Equal(10, mapped.Value);
    }

    [Fact]
    public async Task MapAsync_WithAsyncMapper_TransformsValue()
    {
        // Arrange
        var resultTask = Task.FromResult(ServiceResult<int>.Success(5));

        // Act
        var mapped = await resultTask.MapAsync(async x =>
        {
            await Task.Delay(1);
            return x * 3;
        });

        // Assert
        Assert.True(mapped.IsSuccess);
        Assert.Equal(15, mapped.Value);
    }

    [Fact]
    public async Task BindAsync_ChainsResults_WhenSuccess()
    {
        // Arrange
        var resultTask = Task.FromResult(ServiceResult<int>.Success(10));

        // Act
        var bound = await resultTask.BindAsync(async x =>
        {
            await Task.Delay(1);
            return ServiceResult<string>.Success($"Value: {x}");
        });

        // Assert
        Assert.True(bound.IsSuccess);
        Assert.Equal("Value: 10", bound.Value);
    }

    [Fact]
    public async Task TapAsync_ExecutesAction_WhenSuccess()
    {
        // Arrange
        var resultTask = Task.FromResult(ServiceResult<int>.Success(42));
        var actionExecuted = false;

        // Act
        var result = await resultTask.TapAsync(async _ =>
        {
            await Task.Delay(1);
            actionExecuted = true;
        });

        // Assert
        Assert.True(actionExecuted);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task TapErrorAsync_ExecutesAction_WhenFailure()
    {
        // Arrange
        var resultTask = Task.FromResult(ServiceResult<int>.Failure("Error"));
        var actionExecuted = false;

        // Act
        var result = await resultTask.TapErrorAsync(async _ =>
        {
            await Task.Delay(1);
            actionExecuted = true;
        });

        // Assert
        Assert.True(actionExecuted);
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task MatchAsync_CallsOnSuccess_WhenSuccessful()
    {
        // Arrange
        var resultTask = Task.FromResult(ServiceResult<int>.Success(10));

        // Act
        var matched = await resultTask.MatchAsync(
            async value =>
            {
                await Task.Delay(1);
                return $"Success: {value}";
            },
            async _ =>
            {
                await Task.Delay(1);
                return "Failure";
            });

        // Assert
        Assert.Equal("Success: 10", matched);
    }

    #endregion

    #region ToServiceResult (non-generic) Tests

    [Fact]
    public void ToServiceResult_WithValue_CreatesSuccessResult()
    {
        // Arrange
        var baseResult = ServiceResult.Success();

        // Act
        var typedResult = baseResult.ToServiceResult(42);

        // Assert
        Assert.True(typedResult.IsSuccess);
        Assert.Equal(42, typedResult.Value);
    }

    [Fact]
    public void ToServiceResult_FromFailure_PropagatesError()
    {
        // Arrange
        var baseResult = ServiceResult.Failure("Error message", ErrorType.Validation, 400);

        // Act
        var typedResult = baseResult.ToServiceResult(42);

        // Assert
        Assert.False(typedResult.IsSuccess);
        Assert.Equal("Error message", typedResult.ErrorMessage);
        Assert.Equal(ErrorType.Validation, typedResult.ErrorType);
        Assert.Equal(400, typedResult.StatusCode);
    }

    #endregion
}
