namespace Craft.Files;

/// <summary>
/// Defines the contract for virus scanning services.
/// </summary>
public interface IVirusScanner
{
    /// <summary>
    /// Scans a file stream for viruses.
    /// </summary>
    /// <param name="stream">The file stream to scan.</param>
    /// <param name="fileName">The name of the file being scanned.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the file is clean, false if a virus was detected.</returns>
    Task<bool> ScanAsync(Stream stream, string fileName, CancellationToken cancellationToken = default);
}

/// <summary>
/// Defines the contract for thumbnail generation services.
/// </summary>
public interface IThumbnailGenerator
{
    /// <summary>
    /// Generates a thumbnail for an image file.
    /// </summary>
    /// <param name="sourceStream">The source image stream.</param>
    /// <param name="thumbnailPath">The path where the thumbnail should be saved.</param>
    /// <param name="width">The thumbnail width.</param>
    /// <param name="height">The thumbnail height.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The path where the thumbnail was saved.</returns>
    Task<string?> GenerateAsync(Stream sourceStream, string thumbnailPath, int width, int height, CancellationToken cancellationToken = default);
}

/// <summary>
/// Defines the contract for file access token services.
/// </summary>
public interface IFileAccessTokenService
{
    /// <summary>
    /// Generates a time-limited access token for a file.
    /// </summary>
    /// <param name="fileId">The unique file identifier.</param>
    /// <param name="expirationMinutes">Token expiration time in minutes.</param>
    /// <returns>The generated access token and its expiration time.</returns>
    (string Token, DateTimeOffset ExpiresAt) GenerateToken(string fileId, int expirationMinutes);

    /// <summary>
    /// Validates an access token.
    /// </summary>
    /// <param name="token">The token to validate.</param>
    /// <param name="fileId">The file identifier to validate against.</param>
    /// <returns>True if the token is valid and not expired, false otherwise.</returns>
    bool ValidateToken(string token, string fileId);
}
