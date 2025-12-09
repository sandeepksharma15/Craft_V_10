using System.ComponentModel;

namespace Craft.Files;

public enum UploadType
{
    [Description(@"Images\Assets")]
    Image,

    [Description(@"Images\ProfilePictures")]
    ProfilePicture,

    [Description("Documents")]
    Document
}
