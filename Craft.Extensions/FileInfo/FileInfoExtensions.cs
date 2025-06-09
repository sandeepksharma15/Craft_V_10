#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Microsoft.Extensions.FileProviders;
#pragma warning restore IDE0130 // Namespace does not match folder structure

public static class FileInfoExtensions
{
    /// <summary>
    /// Determines the appropriate content type (MIME type) for an image file based on its extension.
    /// Uses a pattern matching expression to efficiently map supported extensions to their corresponding content types.
    /// </summary>
    /// <param name="file">The IFileInfo object representing the image file.</param>
    /// <returns>The appropriate content type string for the image file, or "application/octet-stream" if the extension is not recognized.</returns>
    public static string ContentType(this IFileInfo file)
        => file.GetImageExtension() switch
        {
            ImageExtension.Jpg => "image/jpeg",
            ImageExtension.Gif => "image/gif",
            ImageExtension.Png => "image/png",
            ImageExtension.Svg => "image/svg",
            _ => "application/octet-stream",
        };

    /// <summary>
    /// Retrieves the extension of a file, including the leading dot (.), in lowercase.
    /// Handles cases where the file name doesn't contain a dot.
    /// </summary>
    /// <param name="file">The IFileInfo object representing the file.</param>
    /// <returns>The file extension in lowercase, including the leading dot, or an empty string if no extension is found.</returns>
    public static string Extension(this IFileInfo file)
    {
        int index = file.Name.LastIndexOf('.');

        return index < 0 ? string.Empty : file.Name[index..].ToLower();
    }

    /// <summary>
    /// Determines the ImageExtension enumeration value representing the image file type based on its extension.
    /// Leverages pattern matching for concise and readable extension mapping.
    /// </summary>
    /// <param name="file">The IFileInfo object representing the image file.</param>
    /// <returns>The corresponding ImageExtension value for the file's extension, or ImageExtension.Unknown if the extension is not recognized as an image type.</returns>
    public static ImageExtension GetImageExtension(this IFileInfo file)
        => file.Extension() switch
        {
            ".jpg" => ImageExtension.Jpg,
            ".gif" => ImageExtension.Gif,
            ".png" => ImageExtension.Png,
            ".svg" => ImageExtension.Svg,
            _ => ImageExtension.Unknown
        };
}

public enum ImageExtension
{
    Unknown,
    Jpg,
    Gif,
    Png,
    Svg
}
