namespace Craft.Core.Tests.Abstractions;

public class IServiceResultTests
{
    #region Interface Contract Tests

    [Fact]
    public void IServiceResult_IsImplementedByServiceResult()
    {
        // Arrange
        IServiceResult result = ServiceResult.Success();

        // Assert
        Assert.IsType<IServiceResult>(result, exactMatch: false);
    }

    [Fact]
    public void IServiceResult_IsImplementedByServiceResultGeneric()
    {
        // Arrange
        IServiceResult result = ServiceResult<int>.Success(42);

        // Assert
        Assert.IsType<IServiceResult>(result, exactMatch: false);
    }

    [Fact]
    public void IServiceResult_IsImplementedByServerResponse()
    {
        // Arrange
        IServiceResult result = new ServerResponse { IsSuccess = true };

        // Assert
        Assert.IsType<IServiceResult>(result, exactMatch: false);
    }

    [Fact]
    public void IServiceResult_IsImplementedByServerResponseGeneric()
    {
        // Arrange
        IServiceResult result = ServerResponse<int>.Success(42);

        // Assert
        Assert.IsType<IServiceResult>(result, exactMatch: false);
    }

    #endregion

    #region IsSuccess Property Tests

    [Fact]
    public void IsSuccess_ReturnsTrue_WhenOperationSucceeds()
    {
        // Arrange
        IServiceResult result = ServiceResult.Success();

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void IsSuccess_ReturnsFalse_WhenOperationFails()
    {
        // Arrange
        IServiceResult result = ServiceResult.Failure("Error");

        // Assert
        Assert.False(result.IsSuccess);
    }

    #endregion

    #region IsFailure Property Tests

    [Fact]
    public void IsFailure_ReturnsFalse_WhenOperationSucceeds()
    {
        // Arrange
        IServiceResult result = ServiceResult.Success();

        // Assert
        Assert.False(result.IsFailure);
    }

    [Fact]
    public void IsFailure_ReturnsTrue_WhenOperationFails()
    {
        // Arrange
        IServiceResult result = ServiceResult.Failure("Error");

        // Assert
        Assert.True(result.IsFailure);
    }

    #endregion

    #region Errors Property Tests

    [Fact]
    public void Errors_IsNull_WhenNoErrorsExist()
    {
        // Arrange
        IServiceResult result = ServiceResult.Success();

        // Assert
        Assert.Null(result.Errors);
    }

    [Fact]
    public void Errors_ContainsErrors_WhenOperationFails()
    {
        // Arrange
        IServiceResult result = ServiceResult.Failure(["Error 1", "Error 2"]);

        // Assert
        Assert.NotNull(result.Errors);
        Assert.Equal(2, result.Errors.Count);
    }

    #endregion

    #region StatusCode Property Tests

    [Fact]
    public void StatusCode_IsNull_WhenNotSpecified()
    {
        // Arrange
        IServiceResult result = ServiceResult.Success();

        // Assert
        Assert.Null(result.StatusCode);
    }

    [Fact]
    public void StatusCode_HasValue_WhenSpecified()
    {
        // Arrange
        IServiceResult result = ServiceResult.NotFound();

        // Assert
        Assert.Equal(404, result.StatusCode);
    }

    #endregion

    #region HasErrors Property Tests

    [Fact]
    public void HasErrors_ReturnsFalse_WhenErrorsIsNull()
    {
        // Arrange
        IServiceResult result = ServiceResult.Success();

        // Assert
        Assert.False(result.HasErrors);
    }

    [Fact]
    public void HasErrors_ReturnsFalse_WhenErrorsIsEmpty()
    {
        // Arrange
        IServiceResult result = ServiceResult.Failure([]);

        // Assert
        Assert.False(result.HasErrors);
    }

    [Fact]
    public void HasErrors_ReturnsTrue_WhenErrorsExist()
    {
        // Arrange
        IServiceResult result = ServiceResult.Failure(["Error"]);

        // Assert
        Assert.True(result.HasErrors);
    }

    #endregion

    #region Message Property Tests

    [Fact]
    public void Message_IsNull_WhenNoMessageOrErrors()
    {
        // Arrange
        IServiceResult result = ServiceResult.Success();

        // Assert
        Assert.Null(result.Message);
    }

    [Fact]
    public void Message_CombinesErrors_WhenMultipleErrorsExist()
    {
        // Arrange
        IServiceResult result = ServiceResult.Failure(["Error 1", "Error 2"]);

        // Assert
        Assert.Equal("Error 1, Error 2", result.Message);
    }

    #endregion

    #region ErrorType Property Tests

    [Fact]
    public void ErrorType_IsNone_WhenOperationSucceeds()
    {
        // Arrange
        IServiceResult result = ServiceResult.Success();

        // Assert
        Assert.Equal(ErrorType.None, result.ErrorType);
    }

    [Theory]
    [InlineData(ErrorType.Validation)]
    [InlineData(ErrorType.NotFound)]
    [InlineData(ErrorType.Unauthorized)]
    [InlineData(ErrorType.Forbidden)]
    [InlineData(ErrorType.Conflict)]
    [InlineData(ErrorType.Internal)]
    [InlineData(ErrorType.Timeout)]
    public void ErrorType_HasExpectedValue_WhenSpecified(ErrorType expectedType)
    {
        // Arrange
        IServiceResult result = ServiceResult.Failure("Error", expectedType);

        // Assert
        Assert.Equal(expectedType, result.ErrorType);
    }

    #endregion

    #region Polymorphism Tests

    [Fact]
    public void IServiceResult_CanBeUsedPolymorphically()
    {
        // Arrange
        var results = new IServiceResult[]
        {
            ServiceResult.Success(),
            ServiceResult<int>.Success(42),
            ServiceResult.Failure("Error"),
            ServiceResult<string>.NotFound()
        };

        // Act
        var successCount = results.Count(r => r.IsSuccess);
        var failureCount = results.Count(r => r.IsFailure);

        // Assert
        Assert.Equal(2, successCount);
        Assert.Equal(2, failureCount);
    }

    [Fact]
    public void IServiceResult_CanBeProcessedInGenericMethod()
    {
        // Arrange
        IServiceResult successResult = ServiceResult.Success();
        IServiceResult failureResult = ServiceResult.Failure("Error");

        // Act & Assert
        Assert.True(ProcessResult(successResult));
        Assert.False(ProcessResult(failureResult));
    }

    private static bool ProcessResult(IServiceResult result) => result.IsSuccess;

    #endregion
}
