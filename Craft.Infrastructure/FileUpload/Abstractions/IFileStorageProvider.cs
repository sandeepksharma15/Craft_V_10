namespace Craft.Infrastructure.FileUpload;

/// <summary>
/// Defines the contract for file storage providers.
/// </summary>
public interface IFileStorageProvider
{
    /// <summary>
    /// The name of the storage provider (e.g., "local", "azure", "aws").
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Uploads a file to the storage provider.
    /// </summary>
    /// <param name="stream">The file stream to upload.</param>
    /// <param name="fileName">The name of the file.</param>
    /// <param name="folderPath">The folder path where the file should be stored.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The path where the file was stored.</returns>
    Task<string> UploadAsync(Stream stream, string fileName, string folderPath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a file exists in the storage.
    /// </summary>
    /// <param name="filePath">The file path to check.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the file exists, false otherwise.</returns>
    Task<bool> ExistsAsync(string filePath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the full physical path for a file (local storage only).
    /// </summary>
    /// <param name="relativePath">The relative path of the file.</param>
    /// <returns>The full physical path, or null if not applicable.</returns>
    string? GetFullPath(string relativePath);
}
