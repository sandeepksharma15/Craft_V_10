namespace Craft.Core.Common.Constants;

/// <summary>
/// Common error types for service results.
/// </summary>
public enum ErrorType
{
    None = 0,
    Validation = 1,
    NotFound = 2,
    Unauthorized = 3,
    Forbidden = 4,
    Conflict = 5,
    Internal = 6,
    Timeout = 7,
    // Add more as needed
}
