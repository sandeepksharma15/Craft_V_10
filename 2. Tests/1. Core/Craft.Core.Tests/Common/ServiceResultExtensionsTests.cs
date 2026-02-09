using Craft.Core.Common;
using Xunit;

namespace Craft.Core.Tests.Common;

public class ServiceResultExtensionsTests
{
    #region ToServiceResult Tests

    [Fact]
    public void ToServiceResult_ConvertsException_ToFailedHttpServiceResult()
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
        Assert.Null(result.Data);
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
        var result = new HttpServiceResult<int>
        {
            IsSuccess = true,
            Data = 5,
            StatusCode = 200
        };

        // Act
        var mapped = result.Map(x => x * 2);

        // Assert
        Assert.True(mapped.IsSuccess);
        Assert.Equal(10, mapped.Data);
        Assert.Equal(200, mapped.StatusCode);
    }

    [Fact]
    public void Map_OnSuccessResult_CanChangeType()
    {
        // Arrange
        var result = new HttpServiceResult<int>
        {
            IsSuccess = true,
            Data = 42,
            StatusCode = 200
        };

        // Act
        var mapped = result.Map(x => x.ToString());

        // Assert
        Assert.True(mapped.IsSuccess);
        Assert.Equal("42", mapped.Data);
    }

    [Fact]
    public void Map_OnFailureResult_PreservesFailure()
    {
        // Arrange
        var errors = new List<string> { "Error1", "Error2" };
        var result = new HttpServiceResult<int>
        {
            IsSuccess = false,
            Errors = errors,
            StatusCode = 404
        };

        // Act
        var mapped = result.Map(x => x * 2);

        // Assert
        Assert.False(mapped.IsSuccess);
        Assert.Equal(errors, mapped.Errors);
        Assert.Equal(404, mapped.StatusCode);
        Assert.Equal(default, mapped.Data);
    }

    [Fact]
    public void Map_WithNullData_HandlesGracefully()
    {
        // Arrange
        var result = new HttpServiceResult<string?>
        {
            IsSuccess = true,
            Data = null,
            StatusCode = 200
        };

        // Act
        var mapped = result.Map(x => x?.Length);

        // Assert
        Assert.True(mapped.IsSuccess);
        Assert.Null(mapped.Data);
    }

    #endregion

    #region ToHttpServiceResult Tests

    [Fact]
    public void ToHttpServiceResult_ConvertsSuccessResult_Correctly()
    {
        // Arrange
        var result = Result<string>.CreateSuccess("test value");

        // Act
        var httpResult = result.ToHttpServiceResult(201);

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
        var httpResult = result.ToHttpServiceResult(400);

        // Assert
        Assert.False(httpResult.IsSuccess);
        Assert.Equal(default, httpResult.Data);
        Assert.Equal(errors, httpResult.Errors);
        Assert.Equal(400, httpResult.StatusCode);
    }

    [Fact]
    public void ToHttpServiceResult_WithoutStatusCode_LeavesItNull()
    {
        // Arrange
        var result = Result<bool>.CreateSuccess(true);

        // Act
        var httpResult = result.ToHttpServiceResult();

        // Assert
        Assert.True(httpResult.IsSuccess);
        Assert.True(httpResult.Data);
        Assert.Null(httpResult.StatusCode);
    }

    #endregion

    #region FirstError Tests

    [Fact]
    public void FirstError_ReturnsFirstError_WhenErrorsExist()
    {
        // Arrange
        var result = new HttpServiceResult<string>
        {
            Errors = ["First", "Second", "Third"]
        };

        // Act
        var firstError = result.FirstError();

        // Assert
        Assert.Equal("First", firstError);
    }

    [Fact]
    public void FirstError_ReturnsNull_WhenNoErrors()
    {
        // Arrange
        var result = new HttpServiceResult<string>
        {
            Errors = []
        };

        // Act
        var firstError = result.FirstError();

        // Assert
        Assert.Null(firstError);
    }

    [Fact]
    public void FirstError_ReturnsNull_WhenErrorsIsNull()
    {
        // Arrange
        var result = new HttpServiceResult<string>
        {
            Errors = null
        };

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
