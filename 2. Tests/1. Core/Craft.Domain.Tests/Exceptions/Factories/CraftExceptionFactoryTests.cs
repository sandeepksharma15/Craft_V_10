using System.Net;

namespace Craft.Domain.Tests.Factories;

public class CraftExceptionFactoryTests
{
    #region Domain Exception Tests

    [Fact]
    public void NotFound_WithEntityAndKey_CreatesCorrectException()
    {
        // Arrange & Act
        var ex = CraftExceptionFactory.NotFound("User", 123);

        // Assert
        Assert.IsType<NotFoundException>(ex);
        Assert.Equal("Entity \"User\" (123) was not found.", ex.Message);
        Assert.Equal(HttpStatusCode.NotFound, ex.StatusCode);
    }

    [Fact]
    public void NotFound_WithMessage_CreatesCorrectException()
    {
        // Arrange & Act
        var ex = CraftExceptionFactory.NotFound("Resource not found");

        // Assert
        Assert.IsType<NotFoundException>(ex);
        Assert.Equal("Resource not found", ex.Message);
    }

    [Fact]
    public void AlreadyExists_WithEntityAndKey_CreatesCorrectException()
    {
        // Arrange & Act
        var ex = CraftExceptionFactory.AlreadyExists("Product", "SKU-123");

        // Assert
        Assert.IsType<AlreadyExistsException>(ex);
        Assert.Contains("Product", ex.Message);
        Assert.Contains("SKU-123", ex.Message);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, ex.StatusCode);
    }

    [Fact]
    public void Concurrency_WithEntityAndKey_CreatesCorrectException()
    {
        // Arrange & Act
        var ex = CraftExceptionFactory.Concurrency("Order", 456);

        // Assert
        Assert.IsType<ConcurrencyException>(ex);
        Assert.Contains("Order", ex.Message);
        Assert.Contains("456", ex.Message);
        Assert.Equal(HttpStatusCode.Conflict, ex.StatusCode);
    }

    [Fact]
    public void Concurrency_WithVersionDetails_CreatesCorrectException()
    {
        // Arrange & Act
        var ex = CraftExceptionFactory.Concurrency("Invoice", 789, "v1", "v2");

        // Assert
        Assert.IsType<ConcurrencyException>(ex);
        Assert.Contains("v1", ex.Message);
        Assert.Contains("v2", ex.Message);
    }

    [Fact]
    public void Conflict_WithResourceAndReason_CreatesCorrectException()
    {
        // Arrange & Act
        var ex = CraftExceptionFactory.Conflict("Order", "Cannot delete with active shipments");

        // Assert
        Assert.IsType<ConflictException>(ex);
        Assert.Contains("Order", ex.Message);
        Assert.Contains("Cannot delete", ex.Message);
        Assert.Equal(HttpStatusCode.Conflict, ex.StatusCode);
    }

    [Fact]
    public void BadRequest_WithMessage_CreatesCorrectException()
    {
        // Arrange & Act
        var ex = CraftExceptionFactory.BadRequest("Invalid input");

        // Assert
        Assert.IsType<BadRequestException>(ex);
        Assert.Equal("Invalid input", ex.Message);
        Assert.Equal(HttpStatusCode.BadRequest, ex.StatusCode);
    }

    [Fact]
    public void BadRequest_WithErrors_CreatesCorrectException()
    {
        // Arrange
        var errors = new List<string> { "Error 1", "Error 2" };

        // Act
        var ex = CraftExceptionFactory.BadRequest("Validation failed", errors);

        // Assert
        Assert.IsType<BadRequestException>(ex);
        Assert.Equal(2, ex.Errors.Count);
    }

    [Fact]
    public void ValidationFailed_WithValidationErrors_CreatesCorrectException()
    {
        // Arrange
        var validationErrors = new Dictionary<string, string[]>
        {
            { "Email", new[] { "Required", "Invalid format" } },
            { "Age", new[] { "Must be positive" } }
        };

        // Act
        var ex = CraftExceptionFactory.ValidationFailed(validationErrors);

        // Assert
        Assert.IsType<ModelValidationException>(ex);
        Assert.Equal(2, ex.ValidationErrors.Count);
    }

    [Fact]
    public void Gone_WithEntityAndKey_CreatesCorrectException()
    {
        // Arrange & Act
        var ex = CraftExceptionFactory.Gone("Document", 999);

        // Assert
        Assert.IsType<GoneException>(ex);
        Assert.Contains("Document", ex.Message);
        Assert.Contains("999", ex.Message);
        Assert.Equal((HttpStatusCode)410, ex.StatusCode);
    }

    [Fact]
    public void Gone_WithDeletedAt_CreatesCorrectException()
    {
        // Arrange
        var deletedAt = new DateTime(2024, 1, 15);

        // Act
        var ex = CraftExceptionFactory.Gone("Order", 123, deletedAt);

        // Assert
        Assert.IsType<GoneException>(ex);
        Assert.Contains("2024-01-15", ex.Message);
    }

    [Fact]
    public void PreconditionFailed_WithHeaderAndValues_CreatesCorrectException()
    {
        // Arrange & Act
        var ex = CraftExceptionFactory.PreconditionFailed("If-Match", "\"abc\"", "\"xyz\"");

        // Assert
        Assert.IsType<PreconditionFailedException>(ex);
        Assert.Contains("If-Match", ex.Message);
        Assert.Equal((HttpStatusCode)412, ex.StatusCode);
    }

    #endregion

    #region Security Exception Tests

    [Fact]
    public void Unauthorized_WithoutMessage_CreatesCorrectException()
    {
        // Arrange & Act
        var ex = CraftExceptionFactory.Unauthorized();

        // Assert
        Assert.IsType<UnauthorizedException>(ex);
        Assert.Equal(HttpStatusCode.Unauthorized, ex.StatusCode);
    }

    [Fact]
    public void Unauthorized_WithMessage_CreatesCorrectException()
    {
        // Arrange & Act
        var ex = CraftExceptionFactory.Unauthorized("Token expired");

        // Assert
        Assert.IsType<UnauthorizedException>(ex);
        Assert.Equal("Token expired", ex.Message);
    }

    [Fact]
    public void Forbidden_WithMessage_CreatesCorrectException()
    {
        // Arrange & Act
        var ex = CraftExceptionFactory.Forbidden("Access denied");

        // Assert
        Assert.IsType<ForbiddenException>(ex);
        Assert.Equal("Access denied", ex.Message);
        Assert.Equal(HttpStatusCode.Forbidden, ex.StatusCode);
    }

    [Fact]
    public void InvalidCredentials_CreatesCorrectException()
    {
        // Arrange & Act
        var ex = CraftExceptionFactory.InvalidCredentials("Invalid password");

        // Assert
        Assert.IsType<InvalidCredentialsException>(ex);
        Assert.Equal("Invalid password", ex.Message);
        Assert.Equal(HttpStatusCode.Unauthorized, ex.StatusCode);
    }

    #endregion

    #region Infrastructure Exception Tests

    [Fact]
    public void Configuration_WithKeyAndReason_CreatesCorrectException()
    {
        // Arrange & Act
        var ex = CraftExceptionFactory.Configuration("Database:ConnectionString", "Value is missing");

        // Assert
        Assert.IsType<ConfigurationException>(ex);
        Assert.Contains("Database:ConnectionString", ex.Message);
        Assert.Contains("Value is missing", ex.Message);
        Assert.Equal(HttpStatusCode.InternalServerError, ex.StatusCode);
    }

    [Fact]
    public void Database_WithOperationAndDetails_CreatesCorrectException()
    {
        // Arrange & Act
        var ex = CraftExceptionFactory.Database("INSERT", "Unique constraint violation");

        // Assert
        Assert.IsType<DatabaseException>(ex);
        Assert.Contains("INSERT", ex.Message);
        Assert.Contains("Unique constraint", ex.Message);
        Assert.Equal(HttpStatusCode.InternalServerError, ex.StatusCode);
    }

    [Fact]
    public void ExternalService_WithServiceAndStatus_CreatesCorrectException()
    {
        // Arrange & Act
        var ex = CraftExceptionFactory.ExternalService("PaymentGateway", 503, "Service unavailable");

        // Assert
        Assert.IsType<ExternalServiceException>(ex);
        Assert.Contains("PaymentGateway", ex.Message);
        Assert.Contains("503", ex.Message);
        Assert.Equal(HttpStatusCode.BadGateway, ex.StatusCode);
    }

    [Fact]
    public void ExternalService_WithServiceAndDetails_CreatesCorrectException()
    {
        // Arrange & Act
        var ex = CraftExceptionFactory.ExternalService("ShippingAPI", "Connection timeout");

        // Assert
        Assert.IsType<ExternalServiceException>(ex);
        Assert.Contains("ShippingAPI", ex.Message);
    }

    #endregion

    #region Server Exception Tests

    [Fact]
    public void InternalServer_WithoutParameters_CreatesCorrectException()
    {
        // Arrange & Act
        var ex = CraftExceptionFactory.InternalServer();

        // Assert
        Assert.IsType<InternalServerException>(ex);
        Assert.Equal(HttpStatusCode.InternalServerError, ex.StatusCode);
    }

    [Fact]
    public void InternalServer_WithInnerException_CreatesCorrectException()
    {
        // Arrange
        var inner = new Exception("Original error");

        // Act
        var ex = CraftExceptionFactory.InternalServer("Wrapped error", inner);

        // Assert
        Assert.IsType<InternalServerException>(ex);
        Assert.Equal(inner, ex.InnerException);
    }

    [Fact]
    public void BadGateway_WithServiceAndDetails_CreatesCorrectException()
    {
        // Arrange & Act
        var ex = CraftExceptionFactory.BadGateway("ExternalAPI", "Invalid response");

        // Assert
        Assert.IsType<BadGatewayException>(ex);
        Assert.Contains("ExternalAPI", ex.Message);
        Assert.Equal(HttpStatusCode.BadGateway, ex.StatusCode);
    }

    [Fact]
    public void GatewayTimeout_WithServiceAndTimeout_CreatesCorrectException()
    {
        // Arrange & Act
        var ex = CraftExceptionFactory.GatewayTimeout("SlowAPI", 30);

        // Assert
        Assert.IsType<GatewayTimeoutException>(ex);
        Assert.Contains("SlowAPI", ex.Message);
        Assert.Contains("30", ex.Message);
        Assert.Equal(HttpStatusCode.GatewayTimeout, ex.StatusCode);
    }

    [Fact]
    public void ServiceUnavailable_CreatesCorrectException()
    {
        // Arrange & Act
        var ex = CraftExceptionFactory.ServiceUnavailable("Maintenance in progress");

        // Assert
        Assert.IsType<ServiceUnavailableException>(ex);
        Assert.Equal("Maintenance in progress", ex.Message);
        Assert.Equal(HttpStatusCode.ServiceUnavailable, ex.StatusCode);
    }

    #endregion

    #region Client Exception Tests

    [Fact]
    public void NotImplemented_WithFeatureAndDetails_CreatesCorrectException()
    {
        // Arrange & Act
        var ex = CraftExceptionFactory.NotImplemented("PDF Export", "Coming in v2.0");

        // Assert
        Assert.IsType<FeatureNotImplementedException>(ex);
        Assert.Contains("PDF Export", ex.Message);
        Assert.Contains("Coming in v2.0", ex.Message);
        Assert.Equal(HttpStatusCode.NotImplemented, ex.StatusCode);
    }

    [Fact]
    public void TooManyRequests_WithLimitAndPeriod_CreatesCorrectException()
    {
        // Arrange & Act
        var ex = CraftExceptionFactory.TooManyRequests(100, "minute");

        // Assert
        Assert.IsType<TooManyRequestsException>(ex);
        Assert.Contains("100", ex.Message);
        Assert.Contains("minute", ex.Message);
        Assert.Equal((HttpStatusCode)429, ex.StatusCode);
    }

    [Fact]
    public void TooManyRequests_WithRetryAfter_CreatesCorrectException()
    {
        // Arrange & Act
        var ex = CraftExceptionFactory.TooManyRequests(60);

        // Assert
        Assert.IsType<TooManyRequestsException>(ex);
        Assert.Contains("60", ex.Message);
    }

    [Fact]
    public void UnsupportedMediaType_WithMediaTypeAndSupported_CreatesCorrectException()
    {
        // Arrange
        var supportedTypes = new[] { "application/json", "application/xml" };

        // Act
        var ex = CraftExceptionFactory.UnsupportedMediaType("text/csv", supportedTypes);

        // Assert
        Assert.IsType<UnsupportedMediaTypeException>(ex);
        Assert.Contains("text/csv", ex.Message);
        Assert.Equal((HttpStatusCode)415, ex.StatusCode);
    }

    [Fact]
    public void PayloadTooLarge_WithSizes_CreatesCorrectException()
    {
        // Arrange & Act
        var ex = CraftExceptionFactory.PayloadTooLarge(15000000, 10000000);

        // Assert
        Assert.IsType<PayloadTooLargeException>(ex);
        Assert.Contains("bytes", ex.Message);
        Assert.Contains("exceeds", ex.Message);
        Assert.Equal((HttpStatusCode)413, ex.StatusCode);
    }

    [Fact]
    public void PayloadTooLarge_WithResourceType_CreatesCorrectException()
    {
        // Arrange & Act
        var ex = CraftExceptionFactory.PayloadTooLarge("Image", 5000000, 2000000);

        // Assert
        Assert.IsType<PayloadTooLargeException>(ex);
        Assert.Contains("Image", ex.Message);
    }

    #endregion

    #region Utility Method Tests

    [Fact]
    public void FromStandardException_WithArgumentNullException_ReturnsBadRequest()
    {
        // Arrange
        var standardEx = new ArgumentNullException("userId");

        // Act
        var ex = CraftExceptionFactory.FromStandardException(standardEx);

        // Assert
        Assert.IsType<BadRequestException>(ex);
        Assert.Contains("userId", ex.Message);
        Assert.Equal(HttpStatusCode.BadRequest, ex.StatusCode);
    }

    [Fact]
    public void FromStandardException_WithUnauthorizedAccessException_ReturnsUnauthorized()
    {
        // Arrange
        var standardEx = new UnauthorizedAccessException();

        // Act
        var ex = CraftExceptionFactory.FromStandardException(standardEx);

        // Assert
        Assert.IsType<UnauthorizedException>(ex);
        Assert.Equal(HttpStatusCode.Unauthorized, ex.StatusCode);
    }

    [Fact]
    public void FromStandardException_WithKeyNotFoundException_ReturnsNotFound()
    {
        // Arrange
        var standardEx = new KeyNotFoundException("Key not found");

        // Act
        var ex = CraftExceptionFactory.FromStandardException(standardEx);

        // Assert
        Assert.IsType<NotFoundException>(ex);
        Assert.Equal(HttpStatusCode.NotFound, ex.StatusCode);
    }

    [Fact]
    public void FromStandardException_WithNotImplementedException_ReturnsNotImplemented()
    {
        // Arrange
        var standardEx = new NotImplementedException("Feature pending");

        // Act
        var ex = CraftExceptionFactory.FromStandardException(standardEx);

        // Assert
        Assert.IsType<FeatureNotImplementedException>(ex);
        Assert.Equal(HttpStatusCode.NotImplemented, ex.StatusCode);
    }

    [Fact]
    public void FromStandardException_WithTimeoutException_ReturnsGatewayTimeout()
    {
        // Arrange
        var standardEx = new TimeoutException();

        // Act
        var ex = CraftExceptionFactory.FromStandardException(standardEx);

        // Assert
        Assert.IsType<GatewayTimeoutException>(ex);
        Assert.Equal(HttpStatusCode.GatewayTimeout, ex.StatusCode);
    }

    [Fact]
    public void FromStandardException_WithUnknownException_ReturnsInternalServer()
    {
        // Arrange
        var standardEx = new InvalidCastException();

        // Act
        var ex = CraftExceptionFactory.FromStandardException(standardEx);

        // Assert
        Assert.IsType<InternalServerException>(ex);
        Assert.Equal(HttpStatusCode.InternalServerError, ex.StatusCode);
        Assert.Equal(standardEx, ex.InnerException);
    }

    [Fact]
    public void FromStatusCode_With400_ReturnsBadRequest()
    {
        // Arrange & Act
        var ex = CraftExceptionFactory.FromStatusCode(400, "Bad request");

        // Assert
        Assert.IsType<BadRequestException>(ex);
        Assert.Equal(HttpStatusCode.BadRequest, ex.StatusCode);
    }

    [Fact]
    public void FromStatusCode_With404_ReturnsNotFound()
    {
        // Arrange & Act
        var ex = CraftExceptionFactory.FromStatusCode(404, "Not found");

        // Assert
        Assert.IsType<NotFoundException>(ex);
        Assert.Equal(HttpStatusCode.NotFound, ex.StatusCode);
    }

    [Fact]
    public void FromStatusCode_With410_ReturnsGone()
    {
        // Arrange & Act
        var ex = CraftExceptionFactory.FromStatusCode(410, "Gone");

        // Assert
        Assert.IsType<GoneException>(ex);
        Assert.Equal((HttpStatusCode)410, ex.StatusCode);
    }

    [Fact]
    public void FromStatusCode_With412_ReturnsPreconditionFailed()
    {
        // Arrange & Act
        var ex = CraftExceptionFactory.FromStatusCode(412, "Precondition failed");

        // Assert
        Assert.IsType<PreconditionFailedException>(ex);
        Assert.Equal((HttpStatusCode)412, ex.StatusCode);
    }

    [Fact]
    public void FromStatusCode_With413_ReturnsPayloadTooLarge()
    {
        // Arrange & Act
        var ex = CraftExceptionFactory.FromStatusCode(413, "Payload too large");

        // Assert
        Assert.IsType<PayloadTooLargeException>(ex);
        Assert.Equal((HttpStatusCode)413, ex.StatusCode);
    }

    [Fact]
    public void FromStatusCode_With500_ReturnsInternalServer()
    {
        // Arrange & Act
        var ex = CraftExceptionFactory.FromStatusCode(500, "Internal error");

        // Assert
        Assert.IsType<InternalServerException>(ex);
        Assert.Equal(HttpStatusCode.InternalServerError, ex.StatusCode);
    }

    [Fact]
    public void FromStatusCode_WithUnknownCode_ReturnsInternalServer()
    {
        // Arrange & Act
        var ex = CraftExceptionFactory.FromStatusCode(999, "Unknown error");

        // Assert
        Assert.IsType<InternalServerException>(ex);
        Assert.Equal(HttpStatusCode.InternalServerError, ex.StatusCode);
    }

    [Fact]
    public void FromStatusCode_WithErrors_IncludesErrors()
    {
        // Arrange
        var errors = new List<string> { "Error 1", "Error 2" };

        // Act
        var ex = CraftExceptionFactory.FromStatusCode(400, "Bad request", errors);

        // Assert
        Assert.Equal(2, ex.Errors.Count);
        Assert.Contains("Error 1", ex.Errors);
        Assert.Contains("Error 2", ex.Errors);
    }

    #endregion
}

