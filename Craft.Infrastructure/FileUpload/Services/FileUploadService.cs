using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Craft.Infrastructure.FileUpload;

/// <summary>
/// Main file upload service implementation.
/// </summary>
public class FileUploadService : IFileUploadService
{
    private readonly FileUploadOptions _options;
    private readonly IFileStorageProvider _storageProvider;
    private readonly IVirusScanner? _virusScanner;
    private readonly IThumbnailGenerator? _thumbnailGenerator;
    private readonly IFileAccessTokenService? _tokenService;
    private readonly object? _tenant;
    private readonly object? _currentUser;
    private readonly ILogger<FileUploadService> _logger;

    public FileUploadService(
        IOptions<FileUploadOptions> options,
        IFileStorageProvider storageProvider,
        ILogger<FileUploadService> logger,
        IVirusScanner? virusScanner = null,
        IThumbnailGenerator? thumbnailGenerator = null,
        IFileAccessTokenService? tokenService = null,
        object? tenant = null,
        object? currentUser = null)
    {
        _options = options.Value;
        _storageProvider = storageProvider;
        _logger = logger;
        _virusScanner = virusScanner;
        _thumbnailGenerator = thumbnailGenerator;
        _tokenService = tokenService;
        _tenant = tenant;
        _currentUser = currentUser;
    }

    public async Task<FileUploadResult> UploadAsync(
        byte[] data,
        string fileName,
        UploadType uploadType,
        string? contentType = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);

        await using var stream = new MemoryStream(data);
        return await UploadAsync(stream, fileName, uploadType, contentType, cancellationToken);
    }

    public async Task<FileUploadResult> UploadAsync(
        Stream stream,
        string fileName,
        UploadType uploadType,
        string? contentType = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);

        try
        {
            var validation = ValidateFile(stream, fileName, uploadType);
            if (!validation.IsValid)
                return FileUploadResult.Failure(validation.ErrorMessage!);

            if (_options.EnableVirusScanning && _virusScanner != null)
            {
                stream.Position = 0;
                var isClean = await _virusScanner.ScanAsync(stream, fileName, cancellationToken);

                if (!isClean)
                {
                    _logger.LogWarning("Virus detected in file: {FileName}", fileName);
                    return FileUploadResult.Failure("Virus detected in file");
                }

                stream.Position = 0;
            }

            var folderPath = GetFolderPath(uploadType);
            var fileId = Guid.NewGuid().ToString();

            stream.Position = 0;
            var filePath = await _storageProvider.UploadAsync(stream, fileName, folderPath, cancellationToken);

            var metadata = new FileMetadata
            {
                FileName = fileName,
                Extension = Path.GetExtension(fileName),
                SizeInBytes = stream.Length,
                ContentType = contentType,
                UploadType = uploadType,
                TenantId = _options.EnableMultiTenancy ? GetTenantId() : null,
                UploadedBy = GetCurrentUserId(),
                UploadedAt = DateTimeOffset.UtcNow,
                VirusScanPassed = _options.EnableVirusScanning ? true : null
            };

            string? thumbnailPath = null;
            if (_options.EnableThumbnailGeneration && IsImageFile(fileName) && _thumbnailGenerator != null)
            {
                stream.Position = 0;
                var thumbnailFolder = Path.Combine(folderPath, "Thumbnails");
                var thumbnailFileName = $"thumb_{Path.GetFileName(filePath)}";
                var thumbnailFullPath = _storageProvider.GetFullPath(Path.Combine(thumbnailFolder, thumbnailFileName));

                if (thumbnailFullPath != null)
                {
                    thumbnailPath = await _thumbnailGenerator.GenerateAsync(
                        stream,
                        thumbnailFullPath,
                        _options.ThumbnailWidth,
                        _options.ThumbnailHeight,
                        cancellationToken);
                }

                metadata = new FileMetadata
                {
                    FileName = metadata.FileName,
                    Extension = metadata.Extension,
                    SizeInBytes = metadata.SizeInBytes,
                    ContentType = metadata.ContentType,
                    UploadType = metadata.UploadType,
                    TenantId = metadata.TenantId,
                    UploadedBy = metadata.UploadedBy,
                    UploadedAt = metadata.UploadedAt,
                    VirusScanPassed = metadata.VirusScanPassed,
                    ThumbnailPath = thumbnailPath
                };
            }

            string? accessToken = null;
            DateTimeOffset? tokenExpiresAt = null;

            if (_options.UseTimeLimitedTokens && _tokenService != null)
            {
                (accessToken, tokenExpiresAt) = _tokenService.GenerateToken(fileId, _options.TokenExpirationMinutes);
            }

            _logger.LogInformation(
                "File uploaded successfully. FileId: {FileId}, Path: {FilePath}, Type: {UploadType}",
                fileId, filePath, uploadType);

            return FileUploadResult.Success(filePath, fileId, metadata, accessToken, tokenExpiresAt);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file: {FileName}", fileName);
            return FileUploadResult.Failure($"Error uploading file: {ex.Message}");
        }
    }

    public async Task<FileUploadResult> UploadBrowserFileAsync(
        IBrowserFile browserFile,
        UploadType uploadType,
        Action<int>? progressCallback = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(browserFile);

        try
        {
            var typeOptions = GetUploadTypeOptions(uploadType);
            var maxAllowedSize = typeOptions.MaxSizeMB * 1024 * 1024;

            if (browserFile.Size > maxAllowedSize)
                return FileUploadResult.Failure($"File size exceeds the allowed limit of {typeOptions.MaxSizeMB} MB");

            await using var stream = new MemoryStream();
            await using var browserStream = browserFile.OpenReadStream(maxAllowedSize, cancellationToken);

            const int bufferSize = 16 * 1024;
            var buffer = new byte[bufferSize];
            long totalBytesRead = 0;
            int bytesRead;

            while ((bytesRead = await browserStream.ReadAsync(buffer, cancellationToken)) > 0)
            {
                await stream.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
                totalBytesRead += bytesRead;

                if (progressCallback != null && browserFile.Size > 0)
                {
                    var progress = (int)((totalBytesRead * 100) / browserFile.Size);
                    progressCallback(progress);
                }
            }

            stream.Position = 0;
            return await UploadAsync(stream, browserFile.Name, uploadType, browserFile.ContentType, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading browser file: {FileName}", browserFile.Name);
            return FileUploadResult.Failure($"Error uploading file: {ex.Message}");
        }
    }

    public string? GetFullPath(string relativePath)
        => _storageProvider.GetFullPath(relativePath);

    public Task<bool> FileExistsAsync(string filePath, CancellationToken cancellationToken = default)
        => _storageProvider.ExistsAsync(filePath, cancellationToken);

    private (bool IsValid, string? ErrorMessage) ValidateFile(Stream stream, string fileName, UploadType uploadType)
    {
        var typeOptions = GetUploadTypeOptions(uploadType);

        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        if (!typeOptions.AllowedExtensions.Contains(extension))
        {
            return (false, $"File extension '{extension}' is not allowed for {uploadType}. " +
                          $"Allowed extensions: {string.Join(", ", typeOptions.AllowedExtensions)}");
        }

        var maxSizeBytes = typeOptions.MaxSizeMB * 1024 * 1024;
        if (stream.Length > maxSizeBytes)
        {
            return (false, $"File size exceeds the allowed limit of {typeOptions.MaxSizeMB} MB");
        }

        return (true, null);
    }

    private string GetFolderPath(UploadType uploadType)
    {
        var typeOptions = GetUploadTypeOptions(uploadType);
        var folderPath = typeOptions.Folder;

        if (_options.EnableMultiTenancy && _tenant != null)
        {
            var tenantId = GetTenantId() ?? "unknown";
            folderPath = Path.Combine("Tenants", tenantId, folderPath);
        }

        return folderPath;
    }

    private UploadTypeOptions GetUploadTypeOptions(UploadType uploadType)
    {
        var uploadTypeName = uploadType.ToString();

        if (_options.UploadTypes.TryGetValue(uploadTypeName, out var typeOptions))
            return typeOptions;

        _logger.LogWarning("No configuration found for upload type: {UploadType}. Using default configuration.", uploadType);

        return new UploadTypeOptions
        {
            MaxSizeMB = _options.DefaultMaxSizeMB,
            AllowedExtensions = [".*"],
            Folder = uploadType.GetDescription()
        };
    }

    private static bool IsImageFile(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension is ".jpg" or ".jpeg" or ".png" or ".gif" or ".bmp" or ".webp";
    }

    private string? GetTenantId()
    {
        if (_tenant == null)
            return null;

        var tenantType = _tenant.GetType();
        
        var identifierProp = tenantType.GetProperty("Identifier");
        if (identifierProp != null)
        {
            var identifier = identifierProp.GetValue(_tenant) as string;
            if (!string.IsNullOrWhiteSpace(identifier))
                return identifier;
        }

        var idProp = tenantType.GetProperty("Id");
        return idProp?.GetValue(_tenant)?.ToString();
    }

    private string? GetCurrentUserId()
    {
        if (_currentUser == null)
            return null;

        var userType = _currentUser.GetType();
        var idProp = userType.GetProperty("Id");
        return idProp?.GetValue(_currentUser)?.ToString();
    }
}
