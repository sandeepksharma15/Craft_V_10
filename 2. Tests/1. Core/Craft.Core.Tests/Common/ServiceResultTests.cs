namespace Craft.Core.Tests.Common;

public class ServiceResultTests
{
    #region ServiceResult (non-generic) Tests

    [Fact]
    public void Success_CreatesSuccessfulResult()
    {
        // Act
        var result = ServiceResult.Success();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Null(result.ErrorMessage);
        Assert.Null(result.Errors);
        Assert.Equal(ErrorType.None, result.ErrorType);
    }

    [Fact]
    public void Failure_WithErrorMessage_CreatesFailedResult()
    {
        // Act
        var result = ServiceResult.Failure("Something went wrong");

        // Assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal("Something went wrong", result.ErrorMessage);
        Assert.Equal(ErrorType.Validation, result.ErrorType);
    }

    [Fact]
    public void Failure_WithErrorList_CreatesFailedResult()
    {
        // Arrange
        var errors = new List<string> { "Error 1", "Error 2", "Error 3" };

        // Act
        var result = ServiceResult.Failure(errors);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Errors);
        Assert.Equal(3, result.Errors.Count);
        Assert.Equal("Error 1", result.Errors[0]);
    }

    [Fact]
    public void NotFound_CreatesNotFoundResult()
    {
        // Act
        var result = ServiceResult.NotFound("Item not found");

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Item not found", result.ErrorMessage);
        Assert.Equal(ErrorType.NotFound, result.ErrorType);
        Assert.Equal(HttpStatusCodes.NotFound, result.StatusCode);
    }

    [Fact]
    public void Unauthorized_CreatesUnauthorizedResult()
    {
        // Act
        var result = ServiceResult.Unauthorized();

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Unauthorized, result.ErrorType);
        Assert.Equal(HttpStatusCodes.Unauthorized, result.StatusCode);
    }

    [Fact]
    public void Forbidden_CreatesForbiddenResult()
    {
        // Act
        var result = ServiceResult.Forbidden();

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Forbidden, result.ErrorType);
        Assert.Equal(HttpStatusCodes.Forbidden, result.StatusCode);
    }

    [Fact]
    public void InternalError_CreatesInternalErrorResult()
    {
        // Act
        var result = ServiceResult.InternalError("Database connection failed");

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Database connection failed", result.ErrorMessage);
        Assert.Equal(ErrorType.Internal, result.ErrorType);
        Assert.Equal(HttpStatusCodes.InternalServerError, result.StatusCode);
    }

    [Fact]
    public void Conflict_CreatesConflictResult()
    {
        // Act
        var result = ServiceResult.Conflict("Resource already exists");

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Resource already exists", result.ErrorMessage);
        Assert.Equal(ErrorType.Conflict, result.ErrorType);
        Assert.Equal(409, result.StatusCode);
    }

    [Fact]
    public void Timeout_CreatesTimeoutResult()
    {
        // Act
        var result = ServiceResult.Timeout();

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Timeout, result.ErrorType);
        Assert.Equal(408, result.StatusCode);
    }

    [Fact]
    public void FromException_CreatesFailedResultFromException()
    {
        // Arrange
        var exception = new InvalidOperationException("Test exception");

        // Act
        var result = ServiceResult.FromException(exception);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Test exception", result.ErrorMessage);
        Assert.Equal(ErrorType.Internal, result.ErrorType);
        Assert.Equal(500, result.StatusCode);
    }

    [Fact]
    public void Message_CombinesErrorsIntoSingleString()
    {
        // Arrange
        var errors = new List<string> { "Error 1", "Error 2" };
        var result = ServiceResult.Failure(errors);

        // Act
        var message = result.Message;

        // Assert
        Assert.Equal("Error 1, Error 2", message);
    }

    [Fact]
    public void HasErrors_ReturnsTrueWhenErrorsExist()
    {
        // Arrange
        var result = ServiceResult.Failure(["Error 1"]);

        // Assert
        Assert.True(result.HasErrors);
    }

    [Fact]
    public void HasErrors_ReturnsFalseWhenNoErrors()
    {
        // Arrange
        var result = ServiceResult.Success();

        // Assert
        Assert.False(result.HasErrors);
    }

    #endregion

    #region ServiceResult<T> Tests

    [Fact]
    public void GenericSuccess_CreatesSuccessfulResultWithValue()
    {
        // Act
        var result = ServiceResult<int>.Success(42);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Equal(42, result.Value);
    }

    [Fact]
    public void GenericSuccess_WithComplexType_CreatesSuccessfulResult()
    {
        // Arrange
        var data = new TestData { Id = 1, Name = "Test" };

        // Act
        var result = ServiceResult<TestData>.Success(data);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(1, result.Value.Id);
        Assert.Equal("Test", result.Value.Name);
    }

    [Fact]
    public void GenericFailure_WithErrorMessage_CreatesFailedResult()
    {
        // Act
        var result = ServiceResult<int>.Failure("Validation failed");

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Validation failed", result.ErrorMessage);
        Assert.Equal(default, result.Value);
    }

    [Fact]
    public void GenericFailure_WithErrorList_CreatesFailedResult()
    {
        // Arrange
        var errors = new List<string> { "Field is required", "Invalid format" };

        // Act
        var result = ServiceResult<string>.Failure(errors);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Errors);
        Assert.Equal(2, result.Errors.Count);
    }

    [Fact]
    public void GenericNotFound_CreatesNotFoundResult()
    {
        // Act
        var result = ServiceResult<TestData>.NotFound("Entity not found");

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.ErrorType);
        Assert.Equal(HttpStatusCodes.NotFound, result.StatusCode);
    }

    [Fact]
    public void GenericUnauthorized_CreatesUnauthorizedResult()
    {
        // Act
        var result = ServiceResult<TestData>.Unauthorized();

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Unauthorized, result.ErrorType);
    }

    [Fact]
    public void GenericForbidden_CreatesForbiddenResult()
    {
        // Act
        var result = ServiceResult<TestData>.Forbidden();

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Forbidden, result.ErrorType);
    }

    [Fact]
    public void GenericInternalError_CreatesInternalErrorResult()
    {
        // Act
        var result = ServiceResult<int>.InternalError();

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Internal, result.ErrorType);
        Assert.Equal(HttpStatusCodes.InternalServerError, result.StatusCode);
    }

    [Fact]
    public void GenericFromException_CreatesFailedResult()
    {
        // Arrange
        var exception = new ArgumentNullException("parameter");

        // Act
        var result = ServiceResult<int>.FromException(exception);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("parameter", result.ErrorMessage);
        Assert.Equal(ErrorType.Internal, result.ErrorType);
    }

    #endregion

    #region Map Tests

    [Fact]
    public void Map_OnSuccess_TransformsValue()
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
    public void Map_OnSuccess_ChangesType()
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
    public void Map_OnFailure_PropagatesFailure()
    {
        // Arrange
        var result = ServiceResult<int>.Failure("Error", ErrorType.Validation, 400);

        // Act
        var mapped = result.Map(x => x * 2);

        // Assert
        Assert.False(mapped.IsSuccess);
        Assert.Equal(ErrorType.Validation, mapped.ErrorType);
        Assert.Equal(400, mapped.StatusCode);
    }

    #endregion

    #region Bind Tests

    [Fact]
    public void Bind_OnSuccess_ChainsOperations()
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
    public void Bind_OnFailure_PropagatesFailure()
    {
        // Arrange
        var result = ServiceResult<int>.Failure("Original error");

        // Act
        var bound = result.Bind(x => ServiceResult<string>.Success($"Value: {x}"));

        // Assert
        Assert.False(bound.IsSuccess);
        Assert.Equal("Original error", bound.ErrorMessage);
    }

    [Fact]
    public void Bind_ChainedOperations_PropagateFirstFailure()
    {
        // Arrange
        var result = ServiceResult<int>.Success(5);

        // Act
        var bound = result
            .Bind(x => ServiceResult<int>.Failure("First operation failed"))
            .Bind(x => ServiceResult<string>.Success($"Should not reach: {x}"));

        // Assert
        Assert.False(bound.IsSuccess);
        Assert.Equal("First operation failed", bound.ErrorMessage);
    }

    #endregion

    #region ToString Tests

    [Fact]
    public void ToString_OnSuccess_ReturnsJsonRepresentation()
    {
        // Arrange
        var result = ServiceResult.Success();

        // Act
        var json = result.ToString();

        // Assert
        Assert.Contains("\"IsSuccess\":true", json);
    }

    [Fact]
    public void ToString_OnFailure_IncludesErrorInformation()
    {
        // Arrange
        var result = ServiceResult.Failure("Test error");

        // Act
        var json = result.ToString();

        // Assert
        Assert.Contains("\"IsSuccess\":false", json);
        Assert.Contains("Test error", json);
    }

    [Fact]
    public void GenericToString_ContainsIsSuccess()
    {
        // Arrange
        var result = ServiceResult<int>.Success(42);

        // Act
        var json = result.ToString();

        // Assert
        Assert.Contains("\"IsSuccess\":true", json);
    }

    #endregion

    #region StatusCode Tests

    [Fact]
    public void Failure_WithCustomStatusCode_SetsStatusCode()
    {
        // Act
        var result = ServiceResult.Failure("Error", ErrorType.Validation, 422);

        // Assert
        Assert.Equal(422, result.StatusCode);
    }

    [Fact]
    public void GenericFailure_WithCustomStatusCode_SetsStatusCode()
    {
        // Act
        var result = ServiceResult<int>.Failure(["Error"], ErrorType.Validation, 422);

        // Assert
        Assert.Equal(422, result.StatusCode);
    }

    #endregion

    private class TestData
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
