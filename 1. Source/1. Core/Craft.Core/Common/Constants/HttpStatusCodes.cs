namespace Craft.Core;

/// <summary>
/// Common HTTP status codes for service results.
/// </summary>
public static class HttpStatusCodes
{
    public const int Ok = 200;
    public const int Created = 201;
    public const int BadRequest = 400;
    public const int Unauthorized = 401;
    public const int Forbidden = 403;
    public const int NotFound = 404;
    public const int InternalServerError = 500;
    // Add more as needed
}
