using System.ComponentModel;

namespace Craft.Infrastructure.FileUpload;

public enum UploadType
{
    [Description(@"Images\Assets")]
    Image,

    [Description(@"Images\ProfilePictures")]
    ProfilePicture,

    [Description("Documents")]
    Document
}
