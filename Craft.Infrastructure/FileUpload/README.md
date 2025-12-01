# File Upload Services

## Overview

The File Upload Services provide a comprehensive, production-ready solution for handling file uploads in both backend APIs and Blazor frontend applications. The system supports multiple storage providers, file validation, virus scanning, thumbnail generation, multi-tenancy, and time-limited access tokens.

## Features

? **Backend & Frontend Support** - Upload from byte arrays, streams, or Blazor `IBrowserFile`  
? **Configurable via appsettings.json** - All settings with sensible defaults  
? **Multiple Upload Types** - Image, ProfilePicture, Document (extensible)  
? **File Validation** - Extension whitelist and size limits per upload type  
? **Multiple Storage Providers** - Local file system (default), extensible for Azure/AWS  
? **Multi-Tenancy Support** - Automatic tenant-isolated storage folders  
? **Virus Scanning** - Optional integration (extensible)  
? **Thumbnail Generation** - Optional image thumbnail creation  
? **Time-Limited Tokens** - Secure file access with expiring tokens  
? **File Metadata Tracking** - Size, type, uploader, timestamp, etc.  
? **Progress Reporting** - Real-time upload progress for browser files  
? **Comprehensive Logging** - Detailed operation logging with Serilog  
? **Full Test Coverage** - 100% test coverage with xUnit  

## Quick Start

### 1. Configuration

Add to your `appsettings.json`:

```json
{
  "FileUploadOptions": {
    "Provider": "local",
    "BasePath": "Files",
    "DefaultMaxSizeMB": 10,
    "EnableVirusScanning": false,
    "EnableThumbnailGeneration": false,
    "ThumbnailWidth": 150,
    "ThumbnailHeight": 150,
    "UseTimeLimitedTokens": false,
    "TokenExpirationMinutes": 60,
    "EnableMultiTenancy": false,
    "UploadTypes": {
      "Image": {
        "MaxSizeMB": 5,
        "AllowedExtensions": [".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp"],
        "Folder": "Images\\Assets"
      },
      "ProfilePicture": {
        "MaxSizeMB": 2,
        "AllowedExtensions": [".jpg", ".jpeg", ".png"],
        "Folder": "Images\\ProfilePictures"
      },
      "Document": {
        "MaxSizeMB": 10,
        "AllowedExtensions": [".pdf", ".doc", ".docx", ".xls", ".xlsx", ".txt"],
        "Folder": "Documents"
      }
    }
  }
}
```

### 2. Register Services

In your `Program.cs`:

```csharp
// Add file upload services
builder.Services.AddFileUploadServices(builder.Configuration);

var app = builder.Build();
app.Run();
```

### 3. Upload Files

#### Backend (API) - From Byte Array

```csharp
public class FileController : ControllerBase
{
    private readonly IFileUploadService _fileUploadService;

    public FileController(IFileUploadService fileUploadService)
    {
        _fileUploadService = fileUploadService;
    }

    [HttpPost("upload")]
    public async Task<IActionResult> UploadFile(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded");

        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream);
        var fileData = memoryStream.ToArray();

        var result = await _fileUploadService.UploadAsync(
            fileData,
            file.FileName,
            UploadType.Document,
            file.ContentType);

        if (!result.IsSuccess)
            return BadRequest(result.ErrorMessage);

        return Ok(new
        {
            result.FilePath,
            result.FileId,
            result.Metadata
        });
    }
}
```

#### Backend - From Stream

```csharp
[HttpPost("upload-stream")]
public async Task<IActionResult> UploadFileStream(IFormFile file)
{
    using var stream = file.OpenReadStream();
    
    var result = await _fileUploadService.UploadAsync(
        stream,
        file.FileName,
        UploadType.Image,
        file.ContentType);

    return result.IsSuccess 
        ? Ok(result) 
        : BadRequest(result.ErrorMessage);
}
```

#### Frontend (Blazor) - From InputFile

```razor
@inject IFileUploadService FileUploadService

<InputFile OnChange="HandleFileSelected" />
<p>Progress: @_uploadProgress%</p>

@code {
    private int _uploadProgress;

    private async Task HandleFileSelected(InputFileChangeEventArgs e)
    {
        var file = e.File;
        
        var result = await FileUploadService.UploadBrowserFileAsync(
            file,
            UploadType.ProfilePicture,
            progress => _uploadProgress = progress);

        if (result.IsSuccess)
        {
            Console.WriteLine($"File uploaded: {result.FilePath}");
            Console.WriteLine($"File ID: {result.FileId}");
        }
        else
        {
            Console.WriteLine($"Upload failed: {result.ErrorMessage}");
        }
    }
}
```

## Advanced Configuration

### Multi-Tenancy

Enable multi-tenancy in `appsettings.json`:

```json
{
  "FileUploadOptions": {
    "EnableMultiTenancy": true
  }
}
```

Files will be automatically stored in tenant-specific folders:
```
Files/
  Tenants/
    acme/
      Images/
        Assets/
          logo.png
      Documents/
        report.pdf
    initech/
      Images/
        Assets/
          banner.jpg
```

### Time-Limited Access Tokens

Enable secure file access:

```json
{
  "FileUploadOptions": {
    "UseTimeLimitedTokens": true,
    "TokenExpirationMinutes": 60
  }
}
```

Usage:

```csharp
var result = await _fileUploadService.UploadAsync(data, fileName, uploadType);

if (result.IsSuccess && result.AccessToken != null)
{
    // Return token to client
    return Ok(new 
    { 
        FileId = result.FileId,
        AccessToken = result.AccessToken,
        ExpiresAt = result.TokenExpiresAt
    });
}
```

### Custom Storage Provider

Implement `IFileStorageProvider`:

```csharp
public class AzureBlobStorageProvider : IFileStorageProvider
{
    public string Name => "azure";

    public async Task<string> UploadAsync(
        Stream stream, 
        string fileName, 
        string folderPath, 
        CancellationToken cancellationToken = default)
    {
        // Upload to Azure Blob Storage
        var containerClient = _blobServiceClient.GetBlobContainerClient(folderPath);
        await containerClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken);
        
        var blobClient = containerClient.GetBlobClient(fileName);
        await blobClient.UploadAsync(stream, cancellationToken);
        
        return $"{folderPath}/{fileName}";
    }

    public async Task<bool> ExistsAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var parts = filePath.Split('/');
        var containerName = parts[0];
        var blobName = string.Join("/", parts.Skip(1));
        
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        var blobClient = containerClient.GetBlobClient(blobName);
        
        return await blobClient.ExistsAsync(cancellationToken);
    }

    public string? GetFullPath(string relativePath) => null; // Not applicable for cloud storage
}
```

Register the custom provider:

```csharp
builder.Services
    .AddFileUploadServices(builder.Configuration)
    .AddFileStorageProvider<AzureBlobStorageProvider>();
```

### Virus Scanning

Implement `IVirusScanner`:

```csharp
public class ClamAVVirusScanner : IVirusScanner
{
    private readonly IClamAvClient _clamAvClient;

    public ClamAVVirusScanner(IClamAvClient clamAvClient)
    {
        _clamAvClient = clamAvClient;
    }

    public async Task<bool> ScanAsync(
        Stream stream, 
        string fileName, 
        CancellationToken cancellationToken = default)
    {
        var scanResult = await _clamAvClient.ScanAsync(stream, cancellationToken);
        return scanResult.IsClean;
    }
}
```

Register and enable:

```csharp
builder.Services
    .AddFileUploadServices(options => options.EnableVirusScanning = true)
    .AddVirusScanner<ClamAVVirusScanner>();
```

### Thumbnail Generation

Implement `IThumbnailGenerator`:

```csharp
public class ImageSharpThumbnailGenerator : IThumbnailGenerator
{
    public async Task<string?> GenerateAsync(
        Stream sourceStream, 
        string thumbnailPath, 
        int width, 
        int height, 
        CancellationToken cancellationToken = default)
    {
        using var image = await Image.LoadAsync(sourceStream, cancellationToken);
        
        image.Mutate(x => x.Resize(new ResizeOptions
        {
            Size = new Size(width, height),
            Mode = ResizeMode.Crop
        }));

        var directory = Path.GetDirectoryName(thumbnailPath);
        if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory!);

        await image.SaveAsync(thumbnailPath, cancellationToken);
        
        return thumbnailPath;
    }
}
```

Register and enable:

```csharp
builder.Services
    .AddFileUploadServices(options => options.EnableThumbnailGeneration = true)
    .AddThumbnailGenerator<ImageSharpThumbnailGenerator>();
```

## File Metadata

Every successful upload returns detailed metadata:

```csharp
public class FileMetadata
{
    public string FileName { get; init; }           // "report.pdf"
    public string Extension { get; init; }          // ".pdf"
    public long SizeInBytes { get; init; }         // 1048576
    public string? ContentType { get; init; }       // "application/pdf"
    public UploadType UploadType { get; init; }    // UploadType.Document
    public string? TenantId { get; init; }         // "acme" (if multi-tenancy enabled)
    public string? UploadedBy { get; init; }       // "user123" (if current user available)
    public DateTimeOffset UploadedAt { get; init; } // DateTime of upload
    public string? ThumbnailPath { get; init; }    // "Thumbnails/thumb_image.jpg"
    public bool? VirusScanPassed { get; init; }    // true (if virus scanning enabled)
}
```

## Upload Types

Define custom upload types by extending the enum:

```csharp
public enum UploadType
{
    [Description(@"Images\Assets")]
    Image,

    [Description(@"Images\ProfilePictures")]
    ProfilePicture,

    [Description("Documents")]
    Document,
    
    // Add your custom types
    [Description("Videos")]
    Video,
    
    [Description(@"Archives\Backups")]
    Backup
}
```

Configure in `appsettings.json`:

```json
{
  "FileUploadOptions": {
    "UploadTypes": {
      "Video": {
        "MaxSizeMB": 100,
        "AllowedExtensions": [".mp4", ".avi", ".mov", ".wmv"],
        "Folder": "Videos"
      },
      "Backup": {
        "MaxSizeMB": 500,
        "AllowedExtensions": [".zip", ".tar", ".gz", ".7z"],
        "Folder": "Archives\\Backups"
      }
    }
  }
}
```

## Error Handling

```csharp
var result = await _fileUploadService.UploadAsync(data, fileName, uploadType);

if (!result.IsSuccess)
{
    // Handle specific errors
    if (result.ErrorMessage.Contains("not allowed"))
    {
        // Invalid file extension
    }
    else if (result.ErrorMessage.Contains("exceeds the allowed limit"))
    {
        // File too large
    }
    else if (result.ErrorMessage.Contains("Virus detected"))
    {
        // Virus found
    }
    else
    {
        // Other error
    }
}
```

## Testing

The library includes comprehensive test coverage:

```csharp
public class MyFileUploadTests
{
    [Fact]
    public async Task UploadFile_Success()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddFileUploadServices(options => 
        {
            options.Provider = "local";
            options.BasePath = Path.GetTempPath();
        });
        
        var provider = services.BuildServiceProvider();
        var service = provider.GetRequiredService<IFileUploadService>();
        
        var data = "Test file"u8.ToArray();
        
        // Act
        var result = await service.UploadAsync(data, "test.txt", UploadType.Document);
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.FilePath);
    }
}
```

## Migration from Legacy Services

### Old Code (Deprecated)

```csharp
// ? Old way - being removed
var request = new UploadRequest
{
    Data = fileBytes,
    FileName = "file.pdf",
    Extension = ".pdf",
    UploadType = UploadType.Document
};

var uploadService = new UploadService();
var filePath = uploadService.UploadFiles(request);
```

### New Code (Recommended)

```csharp
// ? New way - dependency injection
private readonly IFileUploadService _fileUploadService;

public MyService(IFileUploadService fileUploadService)
{
    _fileUploadService = fileUploadService;
}

public async Task UploadFile(byte[] fileBytes, string fileName)
{
    var result = await _fileUploadService.UploadAsync(
        fileBytes,
        fileName,
        UploadType.Document);
    
    if (result.IsSuccess)
    {
        Console.WriteLine($"Uploaded to: {result.FilePath}");
    }
}
```

## Logging

The service logs all operations:

```
[Information] File uploaded successfully. FileId: abc123, Path: Documents/report.pdf, Type: Document
[Debug] Created directory: C:\Files\Documents
[Warning] Virus detected in file: suspicious.exe
[Error] Error uploading file: test.pdf - System.IO.IOException: Disk full
```

Configure logging in `appsettings.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Craft.Infrastructure.FileUpload": "Information"
    }
  }
}
```

## Performance Tips

1. **Use streams** instead of byte arrays for large files
2. **Enable pooling** for high-traffic scenarios
3. **Configure appropriate timeouts** for uploads
4. **Use async/await** throughout your code
5. **Consider cloud storage** for scalability

## Security Best Practices

1. ? Always validate file extensions
2. ? Set appropriate size limits
3. ? Enable virus scanning in production
4. ? Use time-limited tokens for downloads
5. ? Sanitize file names (handled automatically)
6. ? Isolate tenant files (enable multi-tenancy)
7. ? Log all upload attempts

## API Reference

### IFileUploadService

```csharp
Task<FileUploadResult> UploadAsync(byte[] data, string fileName, UploadType uploadType, string? contentType = null, CancellationToken cancellationToken = default);
Task<FileUploadResult> UploadAsync(Stream stream, string fileName, UploadType uploadType, string? contentType = null, CancellationToken cancellationToken = default);
Task<FileUploadResult> UploadBrowserFileAsync(IBrowserFile browserFile, UploadType uploadType, Action<int>? progressCallback = null, CancellationToken cancellationToken = default);
string? GetFullPath(string relativePath);
Task<bool> FileExistsAsync(string filePath, CancellationToken cancellationToken = default);
```

### IFileStorageProvider

```csharp
string Name { get; }
Task<string> UploadAsync(Stream stream, string fileName, string folderPath, CancellationToken cancellationToken = default);
Task<bool> ExistsAsync(string filePath, CancellationToken cancellationToken = default);
string? GetFullPath(string relativePath);
```

### Extension Methods

```csharp
IServiceCollection AddFileUploadServices(IConfiguration configuration);
IServiceCollection AddFileUploadServices(IConfigurationSection configurationSection);
IServiceCollection AddFileUploadServices(Action<FileUploadOptions> configureOptions);
IServiceCollection AddFileStorageProvider<TProvider>() where TProvider : class, IFileStorageProvider;
IServiceCollection AddVirusScanner<TScanner>() where TScanner : class, IVirusScanner;
IServiceCollection AddThumbnailGenerator<TGenerator>() where TGenerator : class, IThumbnailGenerator;
IServiceCollection AddFileAccessTokenService<TTokenService>() where TTokenService : class, IFileAccessTokenService;
```

## Troubleshooting

### Issue: Files not uploading

- Check `BasePath` in configuration
- Verify directory write permissions
- Check disk space
- Review logs for errors

### Issue: Invalid file extension error

- Check `AllowedExtensions` in configuration for the upload type
- Extensions must include the dot (`.pdf`, not `pdf`)

### Issue: File size limit exceeded

- Increase `MaxSizeMB` for the upload type
- Check server request size limits (`MaxRequestBodySize`)

### Issue: Multi-tenancy not working

- Ensure `EnableMultiTenancy = true` in configuration
- Verify `ITenant` is registered in DI
- Check tenant resolution is working

## License

This project is part of the Craft framework and follows the same license.

---

**Last Updated:** January 2025  
**Version:** 1.0.0  
**Target Framework:** .NET 10
