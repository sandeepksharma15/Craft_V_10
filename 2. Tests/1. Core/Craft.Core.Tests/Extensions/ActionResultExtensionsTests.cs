using Microsoft.AspNetCore.Mvc;

namespace Craft.Core.Tests.Extensions;

public class ActionResultExtensionsTests
{
    #region ToActionResult (non-generic) Tests

    [Fact]
    public void ToActionResult_OnSuccess_ReturnsOkResult()
    {
        // Arrange
        var result = ServiceResult.Success();

        // Act
        var actionResult = result.ToActionResult();

        // Assert
        Assert.IsType<OkResult>(actionResult);
    }

    [Fact]
    public void ToActionResult_OnFailure_ReturnsObjectResultWithError()
    {
        // Arrange
        var result = ServiceResult.Failure("Test error", ErrorType.Validation, 400);

        // Act
        var actionResult = result.ToActionResult();

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(actionResult);
        Assert.Equal(400, objectResult.StatusCode);
    }

    [Fact]
    public void ToActionResult_OnNotFound_Returns404()
    {
        // Arrange
        var result = ServiceResult.NotFound("Item not found");

        // Act
        var actionResult = result.ToActionResult();

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(actionResult);
        Assert.Equal(404, objectResult.StatusCode);
    }

    [Fact]
    public void ToActionResult_OnUnauthorized_Returns401()
    {
        // Arrange
        var result = ServiceResult.Unauthorized();

        // Act
        var actionResult = result.ToActionResult();

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(actionResult);
        Assert.Equal(401, objectResult.StatusCode);
    }

    [Fact]
    public void ToActionResult_OnForbidden_Returns403()
    {
        // Arrange
        var result = ServiceResult.Forbidden();

        // Act
        var actionResult = result.ToActionResult();

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(actionResult);
        Assert.Equal(403, objectResult.StatusCode);
    }

    [Fact]
    public void ToActionResult_OnInternalError_Returns500()
    {
        // Arrange
        var result = ServiceResult.InternalError();

        // Act
        var actionResult = result.ToActionResult();

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(actionResult);
        Assert.Equal(500, objectResult.StatusCode);
    }

    [Fact]
    public void ToActionResult_OnConflict_Returns409()
    {
        // Arrange
        var result = ServiceResult.Conflict();

        // Act
        var actionResult = result.ToActionResult();

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(actionResult);
        Assert.Equal(409, objectResult.StatusCode);
    }

    [Fact]
    public void ToActionResult_OnTimeout_Returns408()
    {
        // Arrange
        var result = ServiceResult.Timeout();

        // Act
        var actionResult = result.ToActionResult();

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(actionResult);
        Assert.Equal(408, objectResult.StatusCode);
    }

    #endregion

    #region ToActionResult<T> (generic) Tests

    [Fact]
    public void ToActionResult_Generic_OnSuccess_ReturnsOkObjectResultWithValue()
    {
        // Arrange
        var result = ServiceResult<string>.Success("test value");

        // Act
        var actionResult = result.ToActionResult();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(actionResult);
        Assert.Equal("test value", okResult.Value);
    }

    [Fact]
    public void ToActionResult_Generic_OnSuccess_WithComplexType_ReturnsValue()
    {
        // Arrange
        var data = new TestDto { Id = 1, Name = "Test" };
        var result = ServiceResult<TestDto>.Success(data);

        // Act
        var actionResult = result.ToActionResult();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(actionResult);
        var returnedData = Assert.IsType<TestDto>(okResult.Value);
        Assert.Equal(1, returnedData.Id);
        Assert.Equal("Test", returnedData.Name);
    }

    [Fact]
    public void ToActionResult_Generic_OnFailure_ReturnsObjectResultWithError()
    {
        // Arrange
        var result = ServiceResult<string>.Failure("Validation failed", ErrorType.Validation, 400);

        // Act
        var actionResult = result.ToActionResult();

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(actionResult);
        Assert.Equal(400, objectResult.StatusCode);
    }

    [Fact]
    public void ToActionResult_Generic_OnNotFound_Returns404()
    {
        // Arrange
        var result = ServiceResult<TestDto>.NotFound("Entity not found");

        // Act
        var actionResult = result.ToActionResult();

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(actionResult);
        Assert.Equal(404, objectResult.StatusCode);
    }

    #endregion

    #region ToTypedActionResult Tests

    [Fact]
    public void ToTypedActionResult_OnSuccess_ReturnsValue()
    {
        // Arrange
        var result = ServiceResult<int>.Success(42);

        // Act
        var actionResult = result.ToTypedActionResult<int>();

        // Assert
        Assert.Equal(42, actionResult.Value);
    }

    [Fact]
    public void ToTypedActionResult_OnFailure_ReturnsErrorResult()
    {
        // Arrange
        var result = ServiceResult<int>.Failure("Error", ErrorType.Validation, 400);

        // Act
        var actionResult = result.ToTypedActionResult<int>();

        // Assert
        Assert.NotNull(actionResult.Result);
        var objectResult = Assert.IsType<ObjectResult>(actionResult.Result);
        Assert.Equal(400, objectResult.StatusCode);
    }

    [Fact]
    public void ToTypedActionResult_NonGeneric_OnSuccess_ReturnsOkResult()
    {
        // Arrange
        IServiceResult result = ServiceResult.Success();

        // Act
        var actionResult = result.ToTypedActionResult<string>();

        // Assert
        Assert.IsType<OkResult>(actionResult.Result);
    }

    #endregion

    #region ToServerResponseResult Tests

    [Fact]
    public void ToServerResponseResult_OnSuccess_ReturnsServerResponseWithData()
    {
        // Arrange
        var result = ServiceResult<string>.Success("test data");

        // Act
        var actionResult = result.ToServerResponseResult();

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(actionResult);
        var response = Assert.IsType<ServerResponse<string>>(objectResult.Value);
        Assert.True(response.IsSuccess);
        Assert.Equal("test data", response.Data);
    }

    [Fact]
    public void ToServerResponseResult_OnFailure_ReturnsServerResponseWithErrors()
    {
        // Arrange
        var result = ServiceResult<string>.Failure(["Error 1", "Error 2"], ErrorType.Validation, 400);

        // Act
        var actionResult = result.ToServerResponseResult();

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(actionResult);
        Assert.Equal(400, objectResult.StatusCode);
        var response = Assert.IsType<ServerResponse<string>>(objectResult.Value);
        Assert.False(response.IsSuccess);
        Assert.Equal(2, response.Errors.Count);
    }

    [Fact]
    public void ToServerResponseResult_NonGeneric_OnSuccess_ReturnsServerResponse()
    {
        // Arrange
        IServiceResult result = ServiceResult.Success();

        // Act
        var actionResult = result.ToServerResponseResult();

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(actionResult);
        var response = Assert.IsType<ServerResponse>(objectResult.Value);
        Assert.True(response.IsSuccess);
    }

    [Fact]
    public void ToServerResponseResult_NonGeneric_OnFailure_ReturnsServerResponseWithErrors()
    {
        // Arrange
        IServiceResult result = ServiceResult.Failure(["Error"], ErrorType.NotFound, 404);

        // Act
        var actionResult = result.ToServerResponseResult();

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(actionResult);
        Assert.Equal(404, objectResult.StatusCode);
    }

    #endregion

    #region ToActionResultWithProblemDetails Tests

    [Fact]
    public void ToActionResultWithProblemDetails_OnSuccess_ReturnsOkObjectResult()
    {
        // Arrange
        var result = ServiceResult<string>.Success("test");

        // Act
        var actionResult = result.ToActionResultWithProblemDetails();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(actionResult);
        Assert.Equal("test", okResult.Value);
    }

    [Fact]
    public void ToActionResultWithProblemDetails_OnFailure_ReturnsProblemDetails()
    {
        // Arrange
        var result = ServiceResult<string>.Failure("Validation error", ErrorType.Validation, 400);

        // Act
        var actionResult = result.ToActionResultWithProblemDetails();

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(actionResult);
        Assert.Equal(400, objectResult.StatusCode);
        var problemDetails = Assert.IsType<ProblemDetails>(objectResult.Value);
        Assert.Equal(400, problemDetails.Status);
        Assert.Contains("Validation error", problemDetails.Detail);
    }

    [Fact]
    public void ToActionResultWithProblemDetails_OnNotFound_ReturnsProblemDetailsWithNotFoundTitle()
    {
        // Arrange
        var result = ServiceResult<string>.NotFound("Entity not found");

        // Act
        var actionResult = result.ToActionResultWithProblemDetails();

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(actionResult);
        Assert.Equal(404, objectResult.StatusCode);
        var problemDetails = Assert.IsType<ProblemDetails>(objectResult.Value);
        Assert.Equal("Resource Not Found", problemDetails.Title);
    }

    [Fact]
    public void ToActionResultWithProblemDetails_WithMultipleErrors_IncludesErrorsInExtensions()
    {
        // Arrange
        var result = ServiceResult<string>.Failure(["Error 1", "Error 2"], ErrorType.Validation, 400);

        // Act
        var actionResult = result.ToActionResultWithProblemDetails();

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(actionResult);
        var problemDetails = Assert.IsType<ProblemDetails>(objectResult.Value);
        Assert.Contains("errors", problemDetails.Extensions.Keys);
    }

    [Fact]
    public void ToActionResultWithProblemDetails_NonGeneric_OnSuccess_ReturnsOkResult()
    {
        // Arrange
        IServiceResult result = ServiceResult.Success();

        // Act
        var actionResult = result.ToActionResultWithProblemDetails();

        // Assert
        Assert.IsType<OkResult>(actionResult);
    }

    [Fact]
    public void ToActionResultWithProblemDetails_NonGeneric_OnFailure_ReturnsProblemDetails()
    {
        // Arrange
        IServiceResult result = ServiceResult.Failure("Error", ErrorType.Internal, 500);

        // Act
        var actionResult = result.ToActionResultWithProblemDetails();

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(actionResult);
        Assert.Equal(500, objectResult.StatusCode);
        var problemDetails = Assert.IsType<ProblemDetails>(objectResult.Value);
        Assert.Equal("Internal Server Error", problemDetails.Title);
    }

    #endregion

    #region Default Status Code Mapping Tests

    [Theory]
    [InlineData(ErrorType.NotFound, 404)]
    [InlineData(ErrorType.Unauthorized, 401)]
    [InlineData(ErrorType.Forbidden, 403)]
    [InlineData(ErrorType.Validation, 400)]
    [InlineData(ErrorType.Conflict, 409)]
    [InlineData(ErrorType.Internal, 500)]
    [InlineData(ErrorType.Timeout, 408)]
    [InlineData(ErrorType.None, 400)]
    public void ToActionResult_WithoutStatusCode_UsesDefaultForErrorType(ErrorType errorType, int expectedStatusCode)
    {
        // Arrange
        var result = ServiceResult<string>.Failure("Error", errorType);

        // Act
        var actionResult = result.ToActionResult();

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(actionResult);
        Assert.Equal(expectedStatusCode, objectResult.StatusCode);
    }

    #endregion

    #region Helper Classes

    private class TestDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
    }

    #endregion
}
