namespace Craft.Infrastructure.FileUpload;

public class UploadRequest
{
    public byte[]? Data { get; set; }

    public string? Extension { get; set; }

    public string? FileName { get; set; }

    public UploadType UploadType { get; set; }
}
