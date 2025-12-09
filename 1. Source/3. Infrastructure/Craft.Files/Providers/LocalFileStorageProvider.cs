using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Craft.Files;

/// <summary>
/// Local file system storage provider implementation.
/// </summary>
public class LocalFileStorageProvider : IFileStorageProvider
{
    private readonly FileUploadOptions _options;
    private readonly ILogger<LocalFileStorageProvider> _logger;

    public string Name => "local";

    public LocalFileStorageProvider(IOptions<FileUploadOptions> options, ILogger<LocalFileStorageProvider> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task<string> UploadAsync(Stream stream, string fileName, string folderPath,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
        ArgumentException.ThrowIfNullOrWhiteSpace(folderPath);

        var fullFolderPath = Path.Combine(Directory.GetCurrentDirectory(), _options.BasePath, folderPath);

        if (!Directory.Exists(fullFolderPath))
        {
            Directory.CreateDirectory(fullFolderPath);
            _logger.LogDebug("Created directory: {FolderPath}", fullFolderPath);
        }

        var safeFileName = GetSafeFileName(fileName);
        var fullPath = Path.Combine(fullFolderPath, safeFileName);

        fullPath = GetNextAvailableFileName(fullPath);

        var relativePath = Path.GetRelativePath(
            Path.Combine(Directory.GetCurrentDirectory(), _options.BasePath),
            fullPath);

        try
        {
            const int bufferSize = 81920;
            await using var fileStream = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None,
                bufferSize, useAsync: true);

            await stream.CopyToAsync(fileStream, bufferSize, cancellationToken);

            _logger.LogInformation("File uploaded successfully: {RelativePath}", relativePath);

            return relativePath;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file: {FileName}", fileName);
            throw new IOException($"Error uploading file: {fileName}", ex);
        }
    }

    public Task<bool> ExistsAsync(string filePath, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        var fullPath = Path.Combine(Directory.GetCurrentDirectory(), _options.BasePath, filePath);
        return Task.FromResult(File.Exists(fullPath));
    }

    public string? GetFullPath(string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
            return null;

        return Path.Combine(Directory.GetCurrentDirectory(), _options.BasePath, relativePath);
    }

    private static string GetSafeFileName(string fileName)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        var safeName = string.Concat(fileName.Split(invalidChars));
        return string.IsNullOrWhiteSpace(safeName) ? "file" : safeName.Trim('"');
    }

    private static string GetNextAvailableFileName(string path)
    {
        if (!File.Exists(path))
            return path;

        var directory = Path.GetDirectoryName(path)!;
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(path);
        var extension = Path.GetExtension(path);

        var counter = 1;
        string newPath;

        do
        {
            var newFileName = $"{fileNameWithoutExtension} ({counter}){extension}";
            newPath = Path.Combine(directory, newFileName);
            counter++;
        }
        while (File.Exists(newPath));

        return newPath;
    }
}
