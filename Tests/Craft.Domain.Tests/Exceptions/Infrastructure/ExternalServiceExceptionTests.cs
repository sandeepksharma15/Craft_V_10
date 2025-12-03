using System.Net;

namespace Craft.Exceptions.Tests.Infrastructure;

public class ExternalServiceExceptionTests
{
    [Fact]
    public void DefaultConstructor_SetsDefaultMessageAndStatusCode()
    {
        // Arrange & Act
        var ex = new ExternalServiceException();

        // Assert
        Assert.Equal("An external service error occurred", ex.Message);
        Assert.Equal(HttpStatusCode.BadGateway, ex.StatusCode);
        Assert.Empty(ex.Errors);
    }

    [Fact]
    public void Constructor_WithMessage_SetsMessageAndStatusCode()
    {
        // Arrange & Act
        var ex = new ExternalServiceException("Payment service unavailable");

        // Assert
        Assert.Equal("Payment service unavailable", ex.Message);
        Assert.Equal(HttpStatusCode.BadGateway, ex.StatusCode);
        Assert.Empty(ex.Errors);
    }

    [Fact]
    public void Constructor_WithMessageAndInnerException_SetsProperties()
    {
        // Arrange
        var inner = new HttpRequestException("Connection refused");

        // Act
        var ex = new ExternalServiceException("Failed to call external API", inner);

        // Assert
        Assert.Equal("Failed to call external API", ex.Message);
        Assert.Equal(inner, ex.InnerException);
        Assert.Equal(HttpStatusCode.BadGateway, ex.StatusCode);
    }

    [Fact]
    public void Constructor_WithMessageAndErrors_SetsAllProperties()
    {
        // Arrange
        var errors = new List<string>
        {
            "Service timeout after 30 seconds",
            "Retry attempts exhausted",
            "Circuit breaker opened"
        };

        // Act
        var ex = new ExternalServiceException("External service call failed", errors);

        // Assert
        Assert.Equal("External service call failed", ex.Message);
        Assert.Equal(errors, ex.Errors);
        Assert.Equal(3, ex.Errors.Count);
        Assert.Contains("Service timeout after 30 seconds", ex.Errors);
        Assert.Contains("Retry attempts exhausted", ex.Errors);
        Assert.Contains("Circuit breaker opened", ex.Errors);
        Assert.Equal(HttpStatusCode.BadGateway, ex.StatusCode);
    }

    [Fact]
    public void Constructor_WithServiceNameAndErrorDetails_SetsFormattedMessage()
    {
        // Arrange & Act
        var ex = new ExternalServiceException("PaymentGateway", "Transaction declined by processor");

        // Assert
        Assert.Equal("External service \"PaymentGateway\" error: Transaction declined by processor", ex.Message);
        Assert.Equal(HttpStatusCode.BadGateway, ex.StatusCode);
        Assert.Empty(ex.Errors);
    }

    [Fact]
    public void Constructor_WithServiceNameStatusCodeAndDetails_SetsFormattedMessage()
    {
        // Arrange & Act
        var ex = new ExternalServiceException("ShippingAPI", 503, "Service temporarily unavailable");

        // Assert
        Assert.Equal("External service \"ShippingAPI\" returned status 503: Service temporarily unavailable", ex.Message);
        Assert.Equal(HttpStatusCode.BadGateway, ex.StatusCode);
        Assert.Empty(ex.Errors);
    }

    [Fact]
    public void Constructor_WithServiceNameAndStatusCode_HandlesVariousStatusCodes()
    {
        // Arrange & Act
        var ex1 = new ExternalServiceException("API1", 400, "Bad request");
        var ex2 = new ExternalServiceException("API2", 401, "Unauthorized");
        var ex3 = new ExternalServiceException("API3", 404, "Not found");
        var ex4 = new ExternalServiceException("API4", 500, "Internal error");
        var ex5 = new ExternalServiceException("API5", 503, "Service unavailable");

        // Assert
        Assert.Contains("400", ex1.Message);
        Assert.Contains("401", ex2.Message);
        Assert.Contains("404", ex3.Message);
        Assert.Contains("500", ex4.Message);
        Assert.Contains("503", ex5.Message);
    }

    [Fact]
    public void Constructor_WithNullErrors_InitializesEmptyErrorsList()
    {
        // Arrange & Act
        var ex = new ExternalServiceException("Service error", (List<string>?)null);

        // Assert
        Assert.NotNull(ex.Errors);
        Assert.Empty(ex.Errors);
    }

    [Fact]
    public void Constructor_WithEmptyErrors_InitializesEmptyErrorsList()
    {
        // Arrange
        var errors = new List<string>();

        // Act
        var ex = new ExternalServiceException("Service error", errors);

        // Assert
        Assert.NotNull(ex.Errors);
        Assert.Empty(ex.Errors);
    }

    [Fact]
    public void StatusCode_IsAlwaysBadGateway()
    {
        // Arrange & Act
        var ex1 = new ExternalServiceException();
        var ex2 = new ExternalServiceException("message");
        var ex3 = new ExternalServiceException("service", "details");
        var ex4 = new ExternalServiceException("service", 500, "details");

        // Assert
        Assert.Equal(HttpStatusCode.BadGateway, ex1.StatusCode);
        Assert.Equal(HttpStatusCode.BadGateway, ex2.StatusCode);
        Assert.Equal(HttpStatusCode.BadGateway, ex3.StatusCode);
        Assert.Equal(HttpStatusCode.BadGateway, ex4.StatusCode);
    }

    [Fact]
    public void Constructor_WithMultipleErrors_PreservesAllErrors()
    {
        // Arrange
        var errors = new List<string>
        {
            "Request timeout after 30 seconds",
            "Retry attempt 1 failed",
            "Retry attempt 2 failed",
            "Retry attempt 3 failed",
            "Circuit breaker opened"
        };

        // Act
        var ex = new ExternalServiceException("Service call failed after retries", errors);

        // Assert
        Assert.Equal(5, ex.Errors.Count);
        Assert.All(errors, error => Assert.Contains(error, ex.Errors));
    }

    [Fact]
    public void Constructor_InheritsFromCraftException()
    {
        // Arrange & Act
        var ex = new ExternalServiceException();

        // Assert
        Assert.IsType<CraftException>(ex, exactMatch: false);
        Assert.IsType<Exception>(ex, exactMatch: false);
    }

    [Fact]
    public void Constructor_WithHttpRequestException_PreservesInnerException()
    {
        // Arrange
        var inner = new HttpRequestException("No such host is known");

        // Act
        var ex = new ExternalServiceException("Failed to reach external service", inner);

        // Assert
        Assert.Equal(inner, ex.InnerException);
        Assert.Contains("Failed to reach external service", ex.Message);
    }

    [Fact]
    public void Constructor_WithLongServiceName_PreservesFullName()
    {
        // Arrange
        var longServiceName = "ThirdPartyPaymentProcessingGatewayIntegrationService";

        // Act
        var ex = new ExternalServiceException(longServiceName, "Connection timeout");

        // Assert
        Assert.Contains(longServiceName, ex.Message);
        Assert.Equal(HttpStatusCode.BadGateway, ex.StatusCode);
    }

    [Fact]
    public void Constructor_WithDetailedApiError_PreservesFullDetails()
    {
        // Arrange
        var detailedError = "Payment declined: insufficient funds. Transaction ID: TXN-12345. Please contact your bank.";

        // Act
        var ex = new ExternalServiceException("PaymentAPI", 402, detailedError);

        // Assert
        Assert.Contains(detailedError, ex.Message);
        Assert.Contains("402", ex.Message);
        Assert.Equal(HttpStatusCode.BadGateway, ex.StatusCode);
    }

    [Fact]
    public void Constructor_WithSpecialCharactersInServiceName_HandlesCorrectly()
    {
        // Arrange & Act
        var ex = new ExternalServiceException("Payment-Gateway-V2", "Service error");

        // Assert
        Assert.Contains("Payment-Gateway-V2", ex.Message);
        Assert.Equal(HttpStatusCode.BadGateway, ex.StatusCode);
    }

    [Fact]
    public void Constructor_WithZeroStatusCode_HandlesCorrectly()
    {
        // Arrange & Act
        var ex = new ExternalServiceException("API", 0, "Connection failed before status received");

        // Assert
        Assert.Contains("0", ex.Message);
        Assert.Equal(HttpStatusCode.BadGateway, ex.StatusCode);
    }

    [Fact]
    public void Constructor_WithNegativeStatusCode_HandlesCorrectly()
    {
        // Arrange & Act
        var ex = new ExternalServiceException("API", -1, "Invalid status code");

        // Assert
        Assert.Contains("-1", ex.Message);
        Assert.Equal(HttpStatusCode.BadGateway, ex.StatusCode);
    }

    [Fact]
    public void Constructor_WithCommonHttpStatusCodes_FormatsCorrectly()
    {
        // Arrange & Act
        var ex200 = new ExternalServiceException("API", 200, "Unexpected success response with error");
        var ex404 = new ExternalServiceException("API", 404, "Resource not found");
        var ex429 = new ExternalServiceException("API", 429, "Rate limit exceeded");

        // Assert
        Assert.Contains("200", ex200.Message);
        Assert.Contains("404", ex404.Message);
        Assert.Contains("429", ex429.Message);
        Assert.All([ex200, ex404, ex429], ex => Assert.Equal(HttpStatusCode.BadGateway, ex.StatusCode));
    }
}
