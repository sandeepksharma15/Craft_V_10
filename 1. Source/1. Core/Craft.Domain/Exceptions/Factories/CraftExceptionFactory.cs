namespace Craft.Exceptions;

/// <summary>
/// Factory for creating Craft exceptions with consistent formatting and simplified usage.
/// Provides static methods for all exception types with intelligent defaults.
/// </summary>
public static class CraftExceptionFactory
{
    #region Domain Exceptions

    /// <summary>
    /// Creates a NotFoundException for an entity that was not found.
    /// </summary>
    public static NotFoundException NotFound(string entityName, object key)
        => new(entityName, key);

    /// <summary>
    /// Creates a NotFoundException with a custom message.
    /// </summary>
    public static NotFoundException NotFound(string message)
        => new(message);

    /// <summary>
    /// Creates a NotFoundException with message and error details.
    /// </summary>
    public static NotFoundException NotFound(string message, List<string> errors)
        => new(message, errors);

    /// <summary>
    /// Creates an AlreadyExistsException for an entity that already exists.
    /// </summary>
    public static AlreadyExistsException AlreadyExists(string entityName, object key)
        => new(entityName, key);

    /// <summary>
    /// Creates an AlreadyExistsException with a custom message.
    /// </summary>
    public static AlreadyExistsException AlreadyExists(string message)
        => new(message);

    /// <summary>
    /// Creates a ConcurrencyException for an entity with a concurrency conflict.
    /// </summary>
    public static ConcurrencyException Concurrency(string entityName, object key)
        => new(entityName, key);

    /// <summary>
    /// Creates a ConcurrencyException with version details.
    /// </summary>
    public static ConcurrencyException Concurrency(string entityName, object key, 
        string expectedVersion, string actualVersion)
        => new(entityName, key, expectedVersion, actualVersion);

    /// <summary>
    /// Creates a ConcurrencyException with a custom message.
    /// </summary>
    public static ConcurrencyException Concurrency(string message, List<string>? errors = null)
        => new(message, errors ?? []);

    /// <summary>
    /// Creates a ConflictException with resource name and reason.
    /// </summary>
    public static ConflictException Conflict(string resourceName, string reason)
        => new(resourceName, reason);

    /// <summary>
    /// Creates a ConflictException with a custom message.
    /// </summary>
    public static ConflictException Conflict(string message, List<string>? errors = null)
        => new(message, errors ?? []);

    /// <summary>
    /// Creates a BadRequestException with a custom message.
    /// </summary>
    public static BadRequestException BadRequest(string message, List<string>? errors = null)
        => new(message, errors ?? []);

    /// <summary>
    /// Creates a ModelValidationException with validation errors.
    /// </summary>
    public static ModelValidationException ValidationFailed(IDictionary<string, string[]> validationErrors)
        => new("One or more validation failures have occurred.", validationErrors);

    /// <summary>
    /// Creates a ModelValidationException with custom message and validation errors.
    /// </summary>
    public static ModelValidationException ValidationFailed(string message, 
        IDictionary<string, string[]> validationErrors)
        => new(message, validationErrors);

    /// <summary>
    /// Creates a GoneException for a permanently deleted entity.
    /// </summary>
    public static GoneException Gone(string entityName, object key)
        => new(entityName, key);

    /// <summary>
    /// Creates a GoneException with deletion timestamp.
    /// </summary>
    public static GoneException Gone(string entityName, object key, DateTime deletedAt)
        => new(entityName, key, deletedAt);

    /// <summary>
    /// Creates a GoneException with a custom message.
    /// </summary>
    public static GoneException Gone(string message)
        => new(message);

    /// <summary>
    /// Creates a PreconditionFailedException for failed conditional requests.
    /// </summary>
    public static PreconditionFailedException PreconditionFailed(string headerName, 
        string expectedValue, string actualValue)
        => new(headerName, expectedValue, actualValue);

    /// <summary>
    /// Creates a PreconditionFailedException with a custom message.
    /// </summary>
    public static PreconditionFailedException PreconditionFailed(string message)
        => new(message);

    #endregion

    #region Security Exceptions

    /// <summary>
    /// Creates an UnauthorizedException with optional custom message.
    /// </summary>
    public static UnauthorizedException Unauthorized(string? message = null)
        => message == null ? new() : new(message);

    /// <summary>
    /// Creates an UnauthorizedException with error details.
    /// </summary>
    public static UnauthorizedException Unauthorized(string message, List<string> errors)
        => new(message, errors);

    /// <summary>
    /// Creates a ForbiddenException with optional custom message.
    /// </summary>
    public static ForbiddenException Forbidden(string? message = null, List<string>? errors = null)
        => message == null ? new() : new(message, errors ?? []);

    /// <summary>
    /// Creates an InvalidCredentialsException with optional custom message.
    /// </summary>
    public static InvalidCredentialsException InvalidCredentials(string? message = null)
        => message == null ? new() : new(message);

    /// <summary>
    /// Creates an InvalidCredentialsException with error details.
    /// </summary>
    public static InvalidCredentialsException InvalidCredentials(string message, List<string> errors)
        => new(message, errors);

    #endregion

    #region Infrastructure Exceptions

    /// <summary>
    /// Creates a ConfigurationException for a specific configuration key.
    /// </summary>
    public static ConfigurationException Configuration(string configKey, string reason)
        => new(configKey, reason);

    /// <summary>
    /// Creates a ConfigurationException with a custom message.
    /// </summary>
    public static ConfigurationException Configuration(string message, List<string>? errors = null)
        => new(message, errors ?? []);

    /// <summary>
    /// Creates a DatabaseException for a specific database operation.
    /// </summary>
    public static DatabaseException Database(string operation, string details)
        => new(operation, details);

    /// <summary>
    /// Creates a DatabaseException with a custom message.
    /// </summary>
    public static DatabaseException Database(string message, List<string>? errors = null)
        => new(message, errors ?? []);

    /// <summary>
    /// Creates an ExternalServiceException with service name, status code, and details.
    /// </summary>
    public static ExternalServiceException ExternalService(string serviceName, 
        int statusCode, string errorDetails)
        => new(serviceName, statusCode, errorDetails);

    /// <summary>
    /// Creates an ExternalServiceException with service name and error details.
    /// </summary>
    public static ExternalServiceException ExternalService(string serviceName, string errorDetails)
        => new(serviceName, errorDetails);

    /// <summary>
    /// Creates an ExternalServiceException with a custom message.
    /// </summary>
    public static ExternalServiceException ExternalService(string message, List<string>? errors = null)
        => new(message, errors ?? []);

    #endregion

    #region Server Exceptions

    /// <summary>
    /// Creates an InternalServerException with optional message and inner exception.
    /// </summary>
    public static InternalServerException InternalServer(string? message = null, Exception? innerException = null)
        => message == null ? new() : innerException == null ? new(message) : new(message, innerException);

    /// <summary>
    /// Creates an InternalServerException with error details.
    /// </summary>
    public static InternalServerException InternalServer(string message, List<string> errors)
        => new(message, errors);

    /// <summary>
    /// Creates a BadGatewayException with service name and details.
    /// </summary>
    public static BadGatewayException BadGateway(string serviceName, string details)
        => new(serviceName, details);

    /// <summary>
    /// Creates a BadGatewayException with a custom message.
    /// </summary>
    public static BadGatewayException BadGateway(string message, List<string>? errors = null)
        => new(message, errors ?? []);

    /// <summary>
    /// Creates a GatewayTimeoutException with service name and timeout duration.
    /// </summary>
    public static GatewayTimeoutException GatewayTimeout(string serviceName, int timeoutSeconds)
        => new(serviceName, timeoutSeconds);

    /// <summary>
    /// Creates a GatewayTimeoutException with a custom message.
    /// </summary>
    public static GatewayTimeoutException GatewayTimeout(string message, List<string>? errors = null)
        => new(message, errors ?? []);

    /// <summary>
    /// Creates a ServiceUnavailableException with optional custom message.
    /// </summary>
    public static ServiceUnavailableException ServiceUnavailable(string? message = null)
        => message == null ? new() : new(message);

    /// <summary>
    /// Creates a ServiceUnavailableException with error details.
    /// </summary>
    public static ServiceUnavailableException ServiceUnavailable(string message, List<string> errors)
        => new(message, errors);

    #endregion

    #region Client Exceptions

    /// <summary>
    /// Creates a FeatureNotImplementedException with feature name and details.
    /// </summary>
    public static FeatureNotImplementedException NotImplemented(string featureName, string details)
        => new(featureName, details);

    /// <summary>
    /// Creates a FeatureNotImplementedException with a custom message.
    /// </summary>
    public static FeatureNotImplementedException NotImplemented(string message, List<string>? errors = null)
        => new(message, errors ?? []);

    /// <summary>
    /// Creates a TooManyRequestsException with rate limit details.
    /// </summary>
    public static TooManyRequestsException TooManyRequests(int limit, string period)
        => new(limit, period);

    /// <summary>
    /// Creates a TooManyRequestsException with retry-after seconds.
    /// </summary>
    public static TooManyRequestsException TooManyRequests(int retryAfterSeconds)
        => new(retryAfterSeconds);

    /// <summary>
    /// Creates a TooManyRequestsException with a custom message.
    /// </summary>
    public static TooManyRequestsException TooManyRequests(string message, List<string>? errors = null)
        => new(message, errors ?? []);

    /// <summary>
    /// Creates an UnsupportedMediaTypeException with media type and supported types.
    /// </summary>
    public static UnsupportedMediaTypeException UnsupportedMediaType(string mediaType, 
        string[] supportedTypes)
        => new(mediaType, supportedTypes);

    /// <summary>
    /// Creates an UnsupportedMediaTypeException with a custom message.
    /// </summary>
    public static UnsupportedMediaTypeException UnsupportedMediaType(string message, List<string>? errors = null)
        => new(message, errors ?? []);

    /// <summary>
    /// Creates a PayloadTooLargeException with actual and maximum sizes.
    /// </summary>
    public static PayloadTooLargeException PayloadTooLarge(long actualSize, long maxSize)
        => new(actualSize, maxSize);

    /// <summary>
    /// Creates a PayloadTooLargeException with resource type and sizes.
    /// </summary>
    public static PayloadTooLargeException PayloadTooLarge(string resourceType, long actualSize, long maxSize)
        => new(resourceType, actualSize, maxSize);

    /// <summary>
    /// Creates a PayloadTooLargeException with a custom message.
    /// </summary>
    public static PayloadTooLargeException PayloadTooLarge(string message)
        => new(message);

    #endregion

    #region Utility Methods

    /// <summary>
    /// Converts standard .NET exceptions to appropriate Craft exceptions.
    /// </summary>
    public static CraftException FromStandardException(Exception exception)
    {
        return exception switch
        {
            ArgumentNullException ex => BadRequest($"Argument '{ex.ParamName}' cannot be null"),
            ArgumentException ex => BadRequest(ex.Message),
            InvalidOperationException ex => BadRequest(ex.Message),
            UnauthorizedAccessException => Unauthorized("Access denied"),
            KeyNotFoundException ex => NotFound(ex.Message),
            NotImplementedException ex => NotImplemented("Feature", ex.Message),
            TimeoutException ex => GatewayTimeout(ex.Message),
            OperationCanceledException => GatewayTimeout("Request cancelled or timed out"),
            _ => InternalServer("An unexpected error occurred", exception)
        };
    }

    /// <summary>
    /// Creates an exception with automatic detection based on HTTP status code.
    /// </summary>
    public static CraftException FromStatusCode(int statusCode, string message, List<string>? errors = null)
    {
        return statusCode switch
        {
            400 => BadRequest(message, errors),
            401 => Unauthorized(message),
            403 => Forbidden(message, errors),
            404 => NotFound(message),
            409 => Conflict(message, errors),
            410 => Gone(message),
            412 => PreconditionFailed(message),
            413 => PayloadTooLarge(message),
            415 => UnsupportedMediaType(message, errors),
            422 => AlreadyExists(message),
            429 => TooManyRequests(message, errors),
            500 => InternalServer(message),
            501 => NotImplemented(message, errors),
            502 => BadGateway(message, errors),
            503 => ServiceUnavailable(message),
            504 => GatewayTimeout(message, errors),
            _ => InternalServer(message)
        };
    }

    #endregion
}
