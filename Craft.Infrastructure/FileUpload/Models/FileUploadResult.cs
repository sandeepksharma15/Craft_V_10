namespace Craft.Infrastructure.FileUpload;

/// <summary>
/// Represents the result of a file upload operation.
/// </summary>
public class FileUploadResult
{
    /// <summary>
    /// Indicates whether the upload was successful.
    /// </summary>
    public bool IsSuccess { get; init; }

    /// <summary>
    /// The relative path where the file was saved.
    /// </summary>
    public string? FilePath { get; init; }

    /// <summary>
    /// The unique identifier for the uploaded file.
    /// </summary>
    public string? FileId { get; init; }

    /// <summary>
    /// File metadata information.
    /// </summary>
    public FileMetadata? Metadata { get; init; }

    /// <summary>
    /// Error message if upload failed.
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// The access token for time-limited downloads (if enabled).
    /// </summary>
    public string? AccessToken { get; init; }

    /// <summary>
    /// Token expiration time (if tokens are enabled).
    /// </summary>
    public DateTimeOffset? TokenExpiresAt { get; init; }

    public static FileUploadResult Success(string filePath, string fileId, FileMetadata metadata, string? accessToken = null, DateTimeOffset? tokenExpiresAt = null)
        => new()
        {
            IsSuccess = true,
            FilePath = filePath,
            FileId = fileId,
            Metadata = metadata,
            AccessToken = accessToken,
            TokenExpiresAt = tokenExpiresAt
        };

    public static FileUploadResult Failure(string errorMessage)
        => new()
        {
            IsSuccess = false,
            ErrorMessage = errorMessage
        };
}

/// <summary>
/// Metadata information about an uploaded file.
/// </summary>
public class FileMetadata
{
    /// <summary>
    /// Original file name.
    /// </summary>
    public required string FileName { get; init; }

    /// <summary>
    /// File extension (including the dot).
    /// </summary>
    public required string Extension { get; init; }

    /// <summary>
    /// File size in bytes.
    /// </summary>
    public required long SizeInBytes { get; init; }

    /// <summary>
    /// Content type / MIME type.
    /// </summary>
    public string? ContentType { get; init; }

    /// <summary>
    /// Upload type.
    /// </summary>
    public required UploadType UploadType { get; init; }

    /// <summary>
    /// Tenant identifier (if multi-tenancy is enabled).
    /// </summary>
    public string? TenantId { get; init; }

    /// <summary>
    /// User identifier who uploaded the file.
    /// </summary>
    public string? UploadedBy { get; init; }

    /// <summary>
    /// Date and time when the file was uploaded.
    /// </summary>
    public DateTimeOffset UploadedAt { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Thumbnail path (if generated).
    /// </summary>
    public string? ThumbnailPath { get; init; }

    /// <summary>
    /// Whether the file passed virus scanning (if enabled).
    /// </summary>
    public bool? VirusScanPassed { get; init; }
}
