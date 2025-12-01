namespace Craft.Infrastructure.FileUpload;

/// <summary>
/// Legacy upload request model. Use FileUploadService methods directly instead.
/// </summary>
[Obsolete("Use FileUploadService methods directly instead. This class will be removed in a future version.")]
public class UploadRequest
{
    public byte[]? Data { get; set; }

    public string? Extension { get; set; }

    public string? FileName { get; set; }

    public UploadType UploadType { get; set; }
}
