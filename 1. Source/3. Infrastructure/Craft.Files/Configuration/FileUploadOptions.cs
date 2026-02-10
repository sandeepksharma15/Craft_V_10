using System.ComponentModel.DataAnnotations;

namespace Craft.Files;

/// <summary>
/// Configuration options for file upload services.
/// </summary>
public class FileUploadOptions
{
    public const string SectionName = "FileUploadOptions";

    /// <summary>
    /// Storage provider to use (local, azure, aws, etc.). Default: "local"
    /// </summary>
    [Required]
    public string Provider { get; set; } = "local";

    /// <summary>
    /// Base directory for file storage. Default: "Files"
    /// </summary>
    [Required]
    public string BasePath { get; set; } = "Files";

    /// <summary>
    /// Default maximum file size in MB. Default: 10 MB
    /// </summary>
    [Range(1, 1000)]
    public long DefaultMaxSizeMB { get; set; } = 10;

    /// <summary>
    /// Whether to enable virus scanning. Default: false
    /// </summary>
    public bool EnableVirusScanning { get; set; } = false;

    /// <summary>
    /// Whether to enable automatic thumbnail generation for images. Default: false
    /// </summary>
    public bool EnableThumbnailGeneration { get; set; } = false;

    /// <summary>
    /// Thumbnail width in pixels. Default: 150
    /// </summary>
    [Range(50, 1000)]
    public int ThumbnailWidth { get; set; } = 150;

    /// <summary>
    /// Thumbnail height in pixels. Default: 150
    /// </summary>
    [Range(50, 1000)]
    public int ThumbnailHeight { get; set; } = 150;

    /// <summary>
    /// Whether to generate time-limited access tokens for file downloads. Default: false
    /// </summary>
    public bool UseTimeLimitedTokens { get; set; } = false;

    /// <summary>
    /// Token expiration time in minutes. Default: 60
    /// </summary>
    [Range(1, 1440)]
    public int TokenExpirationMinutes { get; set; } = 60;

    /// <summary>
    /// Whether multi-tenancy is enabled. Default: false
    /// </summary>
    public bool EnableMultiTenancy { get; set; } = false;

    /// <summary>
    /// Upload type-specific configurations
    /// </summary>
    public Dictionary<string, UploadTypeOptions> UploadTypes { get; set; } = new()
    {
        ["Image"] = new UploadTypeOptions
        {
            MaxSizeMB = 5,
            AllowedExtensions = [".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp"],
            Folder = @"Images\Assets"
        },
        ["ProfilePicture"] = new UploadTypeOptions
        {
            MaxSizeMB = 2,
            AllowedExtensions = [".jpg", ".jpeg", ".png"],
            Folder = @"Images\ProfilePictures"
        },
        ["Document"] = new UploadTypeOptions
        {
            MaxSizeMB = 10,
            AllowedExtensions = [".pdf", ".doc", ".docx", ".xls", ".xlsx", ".txt"],
            Folder = "Documents"
        }
    };
}

/// <summary>
/// Configuration options for a specific upload type.
/// </summary>
public class UploadTypeOptions
{
    /// <summary>
    /// Maximum file size in MB for this upload type
    /// </summary>
    [Range(1, 1000)]
    public long MaxSizeMB { get; set; } = 10;

    /// <summary>
    /// Allowed file extensions (including the dot)
    /// </summary>
    [Required]
    [MinLength(1)]
    public List<string> AllowedExtensions { get; set; } = [];

    /// <summary>
    /// Subfolder path for this upload type
    /// </summary>
    [Required]
    public string Folder { get; set; } = string.Empty;
}

