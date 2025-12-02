# Infrastructure Exceptions

This document describes the three infrastructure exception classes in the `Craft.Exceptions` namespace: `ConfigurationException`, `DatabaseException`, and `ExternalServiceException`.

## Overview

Infrastructure exceptions are used to handle errors that occur in the infrastructure layer of your application, including:
- Configuration errors (missing or invalid settings)
- Database operation failures (connection, query, transaction errors)
- External service integration failures (API calls, third-party services)

All infrastructure exceptions inherit from `CraftException` and are automatically handled by the `GlobalExceptionHandler` middleware.

---

## ConfigurationException

### HTTP Status Code: `500 Internal Server Error`

### Description
Thrown when a configuration error occurs, including:
- Missing configuration values
- Invalid configuration formats
- Configuration validation failures
- Configuration file parsing errors

### Constructors

#### 1. Default Constructor
```csharp
throw new ConfigurationException();
// Output: A configuration error occurred
// Status: 500 Internal Server Error
```

#### 2. Custom Message
```csharp
throw new ConfigurationException("Missing required configuration");
// Status: 500 Internal Server Error
```

#### 3. With Inner Exception
```csharp
try
{
    var config = LoadConfiguration();
}
catch (JsonException ex)
{
    throw new ConfigurationException("Failed to parse configuration file", ex);
}
// Status: 500 Internal Server Error
// Inner exception preserved for logging
```

#### 4. With Error List
```csharp
var errors = new List<string>
{
    "ConnectionString is missing",
    "ApiKey is invalid",
    "Timeout value must be positive"
};
throw new ConfigurationException("Configuration validation failed", errors);
// Status: 500 Internal Server Error
// Errors array included in response
```

#### 5. Configuration Key + Reason
```csharp
throw new ConfigurationException("Database:ConnectionString", "Value is missing or empty");
// Output: Configuration error for key "Database:ConnectionString": Value is missing or empty
// Status: 500 Internal Server Error
```

### Usage Examples

#### Startup Configuration Validation
```csharp
public class ConfigurationValidator
{
    public void ValidateConfiguration(IConfiguration configuration)
    {
        var errors = new List<string>();

        if (string.IsNullOrEmpty(configuration["Database:ConnectionString"]))
            errors.Add("Database:ConnectionString is required");

        if (!int.TryParse(configuration["App:Timeout"], out var timeout) || timeout <= 0)
            errors.Add("App:Timeout must be a positive integer");

        if (!Uri.TryCreate(configuration["ExternalApi:BaseUrl"], UriKind.Absolute, out _))
            errors.Add("ExternalApi:BaseUrl must be a valid URL");

        if (errors.Any())
            throw new ConfigurationException("Configuration validation failed", errors);
    }
}
```

#### Options Pattern with Validation
```csharp
public class EmailSettings
{
    public string SmtpHost { get; set; }
    public int SmtpPort { get; set; }
    public string FromAddress { get; set; }

    public void Validate()
    {
        if (string.IsNullOrEmpty(SmtpHost))
            throw new ConfigurationException("EmailSettings:SmtpHost", "SMTP host is required");

        if (SmtpPort <= 0 || SmtpPort > 65535)
            throw new ConfigurationException("EmailSettings:SmtpPort", "Port must be between 1 and 65535");

        if (!MailAddress.TryCreate(FromAddress, out _))
            throw new ConfigurationException("EmailSettings:FromAddress", "Invalid email address format");
    }
}
```

#### Azure Key Vault Integration
```csharp
public class SecretManager
{
    public async Task<string> GetSecretAsync(string secretName)
    {
        try
        {
            var secret = await _keyVaultClient.GetSecretAsync(secretName);
            return secret.Value;
        }
        catch (KeyVaultErrorException ex) when (ex.Response.StatusCode == HttpStatusCode.NotFound)
        {
            throw new ConfigurationException(
                $"Secrets:{secretName}",
                $"Secret not found in Key Vault: {ex.Message}"
            );
        }
        catch (Exception ex)
        {
            throw new ConfigurationException($"Failed to retrieve secret '{secretName}'", ex);
        }
    }
}
```

---

## DatabaseException

### HTTP Status Code: `500 Internal Server Error`

### Description
Thrown when a database operation fails, including:
- Connection failures
- Query execution errors
- Transaction failures
- Constraint violations
- Deadlocks and timeouts

### Constructors

#### 1. Default Constructor
```csharp
throw new DatabaseException();
// Output: A database error occurred
// Status: 500 Internal Server Error
```

#### 2. Custom Message
```csharp
throw new DatabaseException("Failed to connect to database");
// Status: 500 Internal Server Error
```

#### 3. With Inner Exception
```csharp
try
{
    await dbContext.SaveChangesAsync();
}
catch (DbUpdateException ex)
{
    throw new DatabaseException("Failed to save changes", ex);
}
// Status: 500 Internal Server Error
// Inner exception preserved for logging
```

#### 4. With Error List
```csharp
var errors = new List<string>
{
    "Connection pool exhausted",
    "Maximum retry attempts exceeded",
    "Network unreachable"
};
throw new DatabaseException("Database operation failed", errors);
// Status: 500 Internal Server Error
// Errors array included in response
```

#### 5. Operation + Details
```csharp
throw new DatabaseException("INSERT", "Unique constraint violation on column 'Email'");
// Output: Database error during INSERT: Unique constraint violation on column 'Email'
// Status: 500 Internal Server Error
```

### Usage Examples

#### Repository Pattern
```csharp
public class OrderRepository
{
    private readonly AppDbContext _context;

    public async Task<Order> CreateOrderAsync(Order order)
    {
        try
        {
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            return order;
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException sqlEx)
        {
            if (sqlEx.Number == 2627 || sqlEx.Number == 2601) // Unique constraint
                throw new DatabaseException("INSERT", $"Duplicate order number: {order.OrderNumber}");

            if (sqlEx.Number == 547) // Foreign key constraint
                throw new DatabaseException("INSERT", "Invalid foreign key reference");

            throw new DatabaseException("Failed to create order", ex);
        }
        catch (Exception ex)
        {
            throw new DatabaseException("Unexpected error during order creation", ex);
        }
    }

    public async Task DeleteOrderAsync(int orderId)
    {
        try
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null)
                throw new EntityNotFoundException("Order", orderId);

            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            throw new DatabaseException(
                "DELETE",
                "Cannot delete order with active shipments or payments"
            );
        }
    }
}
```

#### Connection Management
```csharp
public class DatabaseHealthCheck : IHealthCheck
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context)
    {
        try
        {
            using var dbContext = await _contextFactory.CreateDbContextAsync();
            await dbContext.Database.CanConnectAsync();
            return HealthCheckResult.Healthy("Database connection successful");
        }
        catch (Exception ex)
        {
            var errors = new List<string>
            {
                "Failed to connect to database",
                $"Error: {ex.Message}",
                "Check connection string and network connectivity"
            };
            throw new DatabaseException("Database health check failed", errors);
        }
    }
}
```

#### Transaction Management
```csharp
public class OrderService
{
    public async Task<Result> ProcessOrderAsync(int orderId)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        
        try
        {
            var order = await _orderRepository.GetByIdAsync(orderId);
            order.Status = OrderStatus.Processing;
            await _orderRepository.UpdateAsync(order);

            var payment = await _paymentService.ProcessPaymentAsync(order.Id);
            
            await _notificationService.SendOrderConfirmationAsync(order.Id);
            
            await transaction.CommitAsync();
            return Result.Success();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            
            var errors = new List<string>
            {
                "Transaction rolled back",
                $"Failed at: {ex.GetType().Name}",
                "Order processing aborted"
            };
            throw new DatabaseException("Order processing transaction failed", errors);
        }
    }
}
```

---

## ExternalServiceException

### HTTP Status Code: `502 Bad Gateway`

### Description
Thrown when an external service call fails, including:
- HTTP service failures
- API errors
- Third-party service errors
- Timeout and network errors
- Authentication failures

### Constructors

#### 1. Default Constructor
```csharp
throw new ExternalServiceException();
// Output: An external service error occurred
// Status: 502 Bad Gateway
```

#### 2. Custom Message
```csharp
throw new ExternalServiceException("Payment service unavailable");
// Status: 502 Bad Gateway
```

#### 3. With Inner Exception
```csharp
try
{
    var response = await httpClient.GetAsync(apiUrl);
    response.EnsureSuccessStatusCode();
}
catch (HttpRequestException ex)
{
    throw new ExternalServiceException("Failed to call external API", ex);
}
// Status: 502 Bad Gateway
// Inner exception preserved for logging
```

#### 4. With Error List
```csharp
var errors = new List<string>
{
    "Service timeout after 30 seconds",
    "Retry attempts exhausted",
    "Circuit breaker opened"
};
throw new ExternalServiceException("External service call failed", errors);
// Status: 502 Bad Gateway
// Errors array included in response
```

#### 5. Service Name + Error Details
```csharp
throw new ExternalServiceException("PaymentGateway", "Transaction declined by processor");
// Output: External service "PaymentGateway" error: Transaction declined by processor
// Status: 502 Bad Gateway
```

#### 6. Service Name + Status Code + Details
```csharp
throw new ExternalServiceException("ShippingAPI", 503, "Service temporarily unavailable");
// Output: External service "ShippingAPI" returned status 503: Service temporarily unavailable
// Status: 502 Bad Gateway
```

### Usage Examples

#### HTTP Client Integration
```csharp
public class PaymentGatewayClient
{
    private readonly HttpClient _httpClient;

    public async Task<PaymentResult> ProcessPaymentAsync(PaymentRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/payments", request);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new ExternalServiceException(
                    "PaymentGateway",
                    (int)response.StatusCode,
                    $"Payment failed: {errorContent}"
                );
            }

            return await response.Content.ReadFromJsonAsync<PaymentResult>();
        }
        catch (HttpRequestException ex)
        {
            throw new ExternalServiceException(
                "PaymentGateway",
                "Unable to reach payment service"
            );
        }
        catch (TaskCanceledException ex)
        {
            throw new ExternalServiceException(
                "PaymentGateway",
                "Request timeout - payment service did not respond"
            );
        }
    }
}
```

#### Retry Policy with Polly
```csharp
public class ResilientHttpClient
{
    private readonly IAsyncPolicy<HttpResponseMessage> _retryPolicy;

    public ResilientHttpClient()
    {
        _retryPolicy = Policy
            .Handle<HttpRequestException>()
            .OrResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (outcome, timespan, retryCount, context) =>
                {
                    _logger.LogWarning($"Retry {retryCount} after {timespan}");
                }
            );
    }

    public async Task<T> GetAsync<T>(string serviceName, string url)
    {
        try
        {
            var response = await _retryPolicy.ExecuteAsync(async () =>
            {
                return await _httpClient.GetAsync(url);
            });

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<T>();
        }
        catch (Exception ex)
        {
            var errors = new List<string>
            {
                "All retry attempts failed",
                $"Last error: {ex.Message}",
                "Service may be down or unreachable"
            };
            throw new ExternalServiceException($"Failed to call {serviceName} after retries", errors);
        }
    }
}
```

#### Circuit Breaker Pattern
```csharp
public class ShippingService
{
    private readonly IAsyncPolicy<HttpResponseMessage> _circuitBreaker;

    public ShippingService()
    {
        _circuitBreaker = Policy
            .Handle<HttpRequestException>()
            .CircuitBreakerAsync(
                exceptionsAllowedBeforeBreaking: 3,
                durationOfBreak: TimeSpan.FromMinutes(1),
                onBreak: (ex, duration) =>
                {
                    _logger.LogError($"Circuit breaker opened for {duration}");
                },
                onReset: () =>
                {
                    _logger.LogInformation("Circuit breaker reset");
                }
            );
    }

    public async Task<ShippingRate> GetShippingRateAsync(ShippingRequest request)
    {
        try
        {
            var response = await _circuitBreaker.ExecuteAsync(async () =>
            {
                return await _httpClient.PostAsJsonAsync("/api/rates", request);
            });

            return await response.Content.ReadFromJsonAsync<ShippingRate>();
        }
        catch (BrokenCircuitException)
        {
            throw new ExternalServiceException(
                "ShippingAPI",
                "Circuit breaker is open - service temporarily unavailable"
            );
        }
        catch (Exception ex)
        {
            throw new ExternalServiceException("ShippingAPI", $"Rate calculation failed: {ex.Message}");
        }
    }
}
```

---

## HTTP Response Format

All infrastructure exceptions return standardized ProblemDetails responses:

### ConfigurationException Response
```json
{
  "type": "https://datatracker.ietf.org/doc/html/rfc9110#section-15.6.1",
  "title": "Internal Server Error",
  "status": 500,
  "detail": "Configuration error for key \"Database:ConnectionString\": Value is missing or empty",
  "instance": "/api/orders",
  "errors": [
    "ConnectionString is missing",
    "ApiKey is invalid"
  ],
  "errorId": "550e8400-e29b-41d4-a716-446655440000",
  "correlationId": "abc-123-def",
  "timestamp": "2024-01-15T10:30:00Z"
}
```

### DatabaseException Response
```json
{
  "type": "https://datatracker.ietf.org/doc/html/rfc9110#section-15.6.1",
  "title": "Internal Server Error",
  "status": 500,
  "detail": "Database error during INSERT: Unique constraint violation on column 'Email'",
  "instance": "/api/users",
  "errors": [
    "Connection pool exhausted",
    "Maximum retry attempts exceeded"
  ],
  "errorId": "550e8400-e29b-41d4-a716-446655440000",
  "correlationId": "abc-123-def",
  "timestamp": "2024-01-15T10:30:00Z"
}
```

### ExternalServiceException Response
```json
{
  "type": "https://datatracker.ietf.org/doc/html/rfc9110#section-15.6.3",
  "title": "Bad Gateway",
  "status": 502,
  "detail": "External service \"PaymentGateway\" returned status 503: Service temporarily unavailable",
  "instance": "/api/payments",
  "errors": [
    "Service timeout after 30 seconds",
    "Retry attempts exhausted"
  ],
  "errorId": "550e8400-e29b-41d4-a716-446655440000",
  "correlationId": "abc-123-def",
  "timestamp": "2024-01-15T10:30:00Z"
}
```

---

## Best Practices

### 1. Configuration Validation at Startup
```csharp
// ? Good - Fail fast with clear error
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        var connectionString = Configuration.GetConnectionString("Default");
        if (string.IsNullOrEmpty(connectionString))
            throw new ConfigurationException("ConnectionStrings:Default", "Connection string is required");
    }
}
```

### 2. Wrap Database Exceptions with Context
```csharp
// ? Good - Provides operation context
catch (DbUpdateException ex)
{
    throw new DatabaseException("UPDATE", $"Failed to update order {orderId}", ex);
}

// ? Bad - Loses context
catch (DbUpdateException ex)
{
    throw new DatabaseException("Database error", ex);
}
```

### 3. Include Service Name in External Service Errors
```csharp
// ? Good - Clear which service failed
throw new ExternalServiceException("PaymentGateway", 503, "Service unavailable");

// ? Bad - Unclear which service
throw new ExternalServiceException("Service error");
```

### 4. Don't Expose Sensitive Information
```csharp
// ? Good - Safe error message
throw new ConfigurationException("Database:ConnectionString", "Value is missing");

// ? Bad - Exposes credentials
throw new ConfigurationException($"Invalid connection: {connectionString}");
```

### 5. Use Error Lists for Multiple Issues
```csharp
// ? Good - All validation errors at once
var errors = ValidateConfiguration(config);
if (errors.Any())
    throw new ConfigurationException("Multiple configuration errors", errors);

// ? Bad - Only first error reported
if (string.IsNullOrEmpty(config.Value1))
    throw new ConfigurationException("Value1 is missing");
```

---

## Related Exceptions

- `InternalServerException` - Generic internal server error (500)
- `BadGatewayException` - Generic bad gateway error (502)
- `ServiceUnavailableException` - Service temporarily unavailable (503)
- `GatewayTimeoutException` - Gateway timeout (504)

---

## See Also

- [RFC 9110 Section 15.6.1 - 500 Internal Server Error](https://datatracker.ietf.org/doc/html/rfc9110#section-15.6.1)
- [RFC 9110 Section 15.6.3 - 502 Bad Gateway](https://datatracker.ietf.org/doc/html/rfc9110#section-15.6.3)
- [Entity Framework Core - Handling Errors](https://learn.microsoft.com/en-us/ef/core/saving/concurrency)
- [Polly - Resilience and Transient Fault Handling](https://github.com/App-vNext/Polly)
