using System.Net;
using System.Text.Json;

namespace Craft.Domain.Tests.Exceptions;

public class ExceptionInfoTests
{
    #region ExceptionInfo Property Tests

    [Fact]
    public void ExceptionInfo_ShouldAllowSettingAllProperties()
    {
        // Arrange & Act
        var info = new ExceptionInfo
        {
            ExceptionType = "NotFoundException",
            Message = "Entity not found",
            StatusCode = 404,
            Errors = ["Error 1", "Error 2"],
            StackTrace = "at Some.Method()",
            InnerException = new ExceptionInfo
            {
                ExceptionType = "InvalidOperationException",
                Message = "Inner error"
            }
        };

        // Assert
        Assert.Equal("NotFoundException", info.ExceptionType);
        Assert.Equal("Entity not found", info.Message);
        Assert.Equal(404, info.StatusCode);
        Assert.Equal(2, info.Errors?.Count);
        Assert.Equal("at Some.Method()", info.StackTrace);
        Assert.NotNull(info.InnerException);
        Assert.Equal("InvalidOperationException", info.InnerException.ExceptionType);
    }

    [Fact]
    public void ExceptionInfo_ShouldBeJsonSerializable()
    {
        // Arrange
        var info = new ExceptionInfo
        {
            ExceptionType = "BadRequestException",
            Message = "Invalid input",
            StatusCode = 400,
            Errors = ["Field X is required", "Field Y is invalid"]
        };

        // Act
        var json = JsonSerializer.Serialize(info);
        var deserialized = JsonSerializer.Deserialize<ExceptionInfo>(json);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal(info.ExceptionType, deserialized.ExceptionType);
        Assert.Equal(info.Message, deserialized.Message);
        Assert.Equal(info.StatusCode, deserialized.StatusCode);
        Assert.Equal(info.Errors, deserialized.Errors);
    }

    [Fact]
    public void ExceptionInfo_ShouldSerializeWithNestedInnerException()
    {
        // Arrange
        var info = new ExceptionInfo
        {
            ExceptionType = "DatabaseException",
            Message = "Database error",
            StatusCode = 500,
            InnerException = new ExceptionInfo
            {
                ExceptionType = "SqlException",
                Message = "Connection failed"
            }
        };

        // Act
        var json = JsonSerializer.Serialize(info);
        var deserialized = JsonSerializer.Deserialize<ExceptionInfo>(json);

        // Assert
        Assert.NotNull(deserialized?.InnerException);
        Assert.Equal("SqlException", deserialized.InnerException.ExceptionType);
    }

    #endregion
}

public class CraftExceptionToErrorInfoTests
{
    #region ToErrorInfo Basic Tests

    [Fact]
    public void ToErrorInfo_ShouldReturnCorrectExceptionType()
    {
        // Arrange
        var exception = new NotFoundException("Entity not found");

        // Act
        var info = exception.ToErrorInfo();

        // Assert
        Assert.Equal("NotFoundException", info.ExceptionType);
    }

    [Fact]
    public void ToErrorInfo_ShouldReturnCorrectMessage()
    {
        // Arrange
        var message = "The requested resource was not found";
        var exception = new NotFoundException(message);

        // Act
        var info = exception.ToErrorInfo();

        // Assert
        Assert.Equal(message, info.Message);
    }

    [Fact]
    public void ToErrorInfo_ShouldReturnCorrectStatusCode()
    {
        // Arrange
        var exception = new NotFoundException("Not found");

        // Act
        var info = exception.ToErrorInfo();

        // Assert
        Assert.Equal(404, info.StatusCode);
    }

    [Fact]
    public void ToErrorInfo_ShouldIncludeErrors()
    {
        // Arrange
        var errors = new List<string> { "Error 1", "Error 2", "Error 3" };
        var exception = new BadRequestException("Bad request", errors);

        // Act
        var info = exception.ToErrorInfo();

        // Assert
        Assert.NotNull(info.Errors);
        Assert.Equal(3, info.Errors.Count);
        Assert.Contains("Error 1", info.Errors);
        Assert.Contains("Error 2", info.Errors);
        Assert.Contains("Error 3", info.Errors);
    }

    [Fact]
    public void ToErrorInfo_ShouldReturnNullErrors_WhenNoErrors()
    {
        // Arrange
        var exception = new NotFoundException("Not found");

        // Act
        var info = exception.ToErrorInfo();

        // Assert
        Assert.Null(info.Errors);
    }

    #endregion

    #region StackTrace Tests

    [Fact]
    public void ToErrorInfo_ShouldNotIncludeStackTrace_ByDefault()
    {
        // Arrange
        var exception = new NotFoundException("Not found");

        // Act
        var info = exception.ToErrorInfo();

        // Assert
        Assert.Null(info.StackTrace);
    }

    [Fact]
    public void ToErrorInfo_ShouldIncludeStackTrace_WhenRequested()
    {
        // Arrange
        NotFoundException exception;
        try
        {
            throw new NotFoundException("Not found");
        }
        catch (NotFoundException ex)
        {
            exception = ex;
        }

        // Act
        var info = exception.ToErrorInfo(includeStackTrace: true);

        // Assert
        Assert.NotNull(info.StackTrace);
        Assert.Contains("ToErrorInfo_ShouldIncludeStackTrace_WhenRequested", info.StackTrace);
    }

    #endregion

    #region InnerException Tests

    [Fact]
    public void ToErrorInfo_ShouldIncludeCraftExceptionInnerException()
    {
        // Arrange
        var innerException = new NotFoundException("Inner not found");
        var outerException = new BadRequestException("Bad request", innerException);

        // Act
        var info = outerException.ToErrorInfo();

        // Assert
        Assert.NotNull(info.InnerException);
        Assert.Equal("NotFoundException", info.InnerException.ExceptionType);
        Assert.Equal("Inner not found", info.InnerException.Message);
        Assert.Equal(404, info.InnerException.StatusCode);
    }

    [Fact]
    public void ToErrorInfo_ShouldIncludeRegularInnerException()
    {
        // Arrange
        var innerException = new InvalidOperationException("Invalid operation");
        var outerException = new DatabaseException("Database error", innerException);

        // Act
        var info = outerException.ToErrorInfo();

        // Assert
        Assert.NotNull(info.InnerException);
        Assert.Equal("InvalidOperationException", info.InnerException.ExceptionType);
        Assert.Equal("Invalid operation", info.InnerException.Message);
    }

    [Fact]
    public void ToErrorInfo_ShouldReturnNullInnerException_WhenNone()
    {
        // Arrange
        var exception = new NotFoundException("Not found");

        // Act
        var info = exception.ToErrorInfo();

        // Assert
        Assert.Null(info.InnerException);
    }

    #endregion

    #region Different Exception Types Tests

    [Fact]
    public void ToErrorInfo_ShouldWork_ForBadRequestException()
    {
        // Arrange
        var exception = new BadRequestException("Invalid input");

        // Act
        var info = exception.ToErrorInfo();

        // Assert
        Assert.Equal("BadRequestException", info.ExceptionType);
        Assert.Equal(400, info.StatusCode);
    }

    [Fact]
    public void ToErrorInfo_ShouldWork_ForForbiddenException()
    {
        // Arrange
        var exception = new ForbiddenException("Access denied");

        // Act
        var info = exception.ToErrorInfo();

        // Assert
        Assert.Equal("ForbiddenException", info.ExceptionType);
        Assert.Equal(403, info.StatusCode);
    }

    [Fact]
    public void ToErrorInfo_ShouldWork_ForUnauthorizedException()
    {
        // Arrange
        var exception = new UnauthorizedException("Not authorized");

        // Act
        var info = exception.ToErrorInfo();

        // Assert
        Assert.Equal("UnauthorizedException", info.ExceptionType);
        Assert.Equal(401, info.StatusCode);
    }

    [Fact]
    public void ToErrorInfo_ShouldWork_ForInternalServerException()
    {
        // Arrange
        var exception = new InternalServerException("Server error");

        // Act
        var info = exception.ToErrorInfo();

        // Assert
        Assert.Equal("InternalServerException", info.ExceptionType);
        Assert.Equal(500, info.StatusCode);
    }

    #endregion

    #region StatusCodeValue Property Tests

    [Fact]
    public void StatusCodeValue_ShouldReturnIntegerStatusCode()
    {
        // Arrange
        var exception = new NotFoundException("Not found");

        // Act
        var statusCodeValue = exception.StatusCodeValue;

        // Assert
        Assert.Equal(404, statusCodeValue);
        Assert.Equal((int)HttpStatusCode.NotFound, statusCodeValue);
    }

    #endregion

    #region JSON Serialization Integration Tests

    [Fact]
    public void ToErrorInfo_ShouldProduceJsonSerializableResult()
    {
        // Arrange
        var exception = new BadRequestException("Bad request", ["Error 1", "Error 2"]);

        // Act
        var info = exception.ToErrorInfo();
        var json = JsonSerializer.Serialize(info);

        // Assert
        Assert.Contains("BadRequestException", json);
        Assert.Contains("Bad request", json);
        Assert.Contains("Error 1", json);
    }

    [Fact]
    public void ToErrorInfo_ShouldRoundTripThroughJson()
    {
        // Arrange
        var innerEx = new InvalidOperationException("Inner error");
        var exception = new DatabaseException("Database error", innerEx);
        
        // Act
        var info = exception.ToErrorInfo(includeStackTrace: false);
        var json = JsonSerializer.Serialize(info);
        var deserialized = JsonSerializer.Deserialize<ExceptionInfo>(json);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal(info.ExceptionType, deserialized.ExceptionType);
        Assert.Equal(info.Message, deserialized.Message);
        Assert.Equal(info.StatusCode, deserialized.StatusCode);
    }

    #endregion
}
