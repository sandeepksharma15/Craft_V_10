using Microsoft.AspNetCore.Components.Forms;

namespace Craft.Files;

/// <summary>
/// Defines the contract for file upload services.
/// </summary>
public interface IFileUploadService
{
    /// <summary>
    /// Uploads a file from a byte array (backend scenarios).
    /// </summary>
    /// <param name="data">The file data as a byte array.</param>
    /// <param name="fileName">The original file name.</param>
    /// <param name="uploadType">The type of upload.</param>
    /// <param name="contentType">The content type / MIME type of the file.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The result of the upload operation.</returns>
    Task<FileUploadResult> UploadAsync(byte[] data, string fileName, UploadType uploadType, string? contentType = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Uploads a file from a stream (backend scenarios).
    /// </summary>
    /// <param name="stream">The file stream.</param>
    /// <param name="fileName">The original file name.</param>
    /// <param name="uploadType">The type of upload.</param>
    /// <param name="contentType">The content type / MIME type of the file.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The result of the upload operation.</returns>
    Task<FileUploadResult> UploadAsync(Stream stream, string fileName, UploadType uploadType, string? contentType = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Uploads a browser file (Blazor frontend scenarios).
    /// </summary>
    /// <param name="browserFile">The browser file from InputFile component.</param>
    /// <param name="uploadType">The type of upload.</param>
    /// <param name="progressCallback">Optional callback to report upload progress (0-100).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The result of the upload operation.</returns>
    Task<FileUploadResult> UploadBrowserFileAsync(IBrowserFile browserFile, UploadType uploadType,
        Action<int>? progressCallback = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the full physical path for a file (local storage only).
    /// </summary>
    /// <param name="relativePath">The relative path of the file.</param>
    /// <returns>The full physical path, or null if not applicable.</returns>
    string? GetFullPath(string relativePath);

    /// <summary>
    /// Checks if a file exists.
    /// </summary>
    /// <param name="filePath">The file path to check.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the file exists, false otherwise.</returns>
    Task<bool> FileExistsAsync(string filePath, CancellationToken cancellationToken = default);
}
