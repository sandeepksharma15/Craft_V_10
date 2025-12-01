namespace Craft.Infrastructure.FileUpload;

/// <summary>
/// Legacy interface for file uploads. Use IFileUploadService instead.
/// </summary>
[Obsolete("Use IFileUploadService instead. This interface will be removed in a future version.")]
public interface IUploadService
{
    string UploadFiles(UploadRequest model);
}
