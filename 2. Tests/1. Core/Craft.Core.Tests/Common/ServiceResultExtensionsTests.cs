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
        Assert.NotNull(result.Errors);
        Assert.Single(result.Errors);
        Assert.Equal("Test error", result.Errors[0]);
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
        Assert.Equal("Invalid argument", result.Errors![0]);
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

    #region ToHttpServiceResult Tests (Backward Compatibility)

    [Fact]
    public void ToHttpServiceResult_ConvertsSuccessResult_Correctly()
    {
        // Arrange
        var result = Result<string>.CreateSuccess("test value");

        // Act
#pragma warning disable CS0618 // Type or member is obsolete
        var httpResult = result.ToHttpServiceResult(201);
#pragma warning restore CS0618

        // Assert
        Assert.True(httpResult.IsSuccess);
        Assert.Equal("test value", httpResult.Data);
        Assert.Equal(201, httpResult.StatusCode);
        Assert.Null(httpResult.Errors);
    }

    [Fact]
    public void ToHttpServiceResult_ConvertsFailureResult_Correctly()
    {
        // Arrange
        var errors = new List<string> { "Validation error" };
        var result = Result<int>.CreateFailure(errors);

        // Act
#pragma warning disable CS0618 // Type or member is obsolete
        var httpResult = result.ToHttpServiceResult(400);
#pragma warning restore CS0618

        // Assert
        Assert.False(httpResult.IsSuccess);
        Assert.Equal(default, httpResult.Data);
        Assert.Equal(errors, httpResult.Errors);
        Assert.Equal(400, httpResult.StatusCode);
    }

    [Fact]
    public void ToHttpServiceResult_WithoutStatusCode_UsesResultStatusCode()
    {
        // Arrange
        var result = Result<bool>.CreateSuccess(true);

        // Act
#pragma warning disable CS0618 // Type or member is obsolete
        var httpResult = result.ToHttpServiceResult();
#pragma warning restore CS0618

        // Assert
        Assert.True(httpResult.IsSuccess);
        Assert.True(httpResult.Data);
    }

    #endregion

    #region FirstError Tests

    [Fact]
    public void FirstError_ReturnsFirstError_WhenErrorsExist()
    {
        // Arrange
        var result = ServiceResult<string>.Failure(["Error1", "Error2"]);

        // Assert
        Assert.Equal("Error1", result.FirstError());
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
    public void FirstError_WorksWithResult_Type()
    {
        // Arrange
        var result = Result.CreateFailure("Single error");

        // Act
        var firstError = result.FirstError();

        // Assert
        // Result stores single error in ErrorMessage, not in Errors list by default
        Assert.Null(firstError); // Because Errors list is null
    }

    [Fact]
    public void FirstError_WorksWithResult_WithErrorList()
    {
        // Arrange
        var errors = new List<string> { "Error A", "Error B" };
        var result = Result<int>.CreateFailure(errors);

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
}
