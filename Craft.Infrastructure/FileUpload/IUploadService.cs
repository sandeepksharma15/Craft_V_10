namespace Craft.Infrastructure.FileUpload;

public interface IUploadService
{
    string UploadFiles(UploadRequest model);
}
