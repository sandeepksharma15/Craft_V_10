# File Upload Services - Quick Start Guide

## 1. Installation

The file upload services are part of `Craft.Infrastructure` package. No additional installation required.

## 2. Configuration

Add to your `appsettings.json` (or use defaults):

```json
{
  "FileUploadOptions": {
    "Provider": "local",
    "BasePath": "Files",
    "DefaultMaxSizeMB": 10,
    "EnableMultiTenancy": false
  }
}
```

## 3. Register Services

In your `Program.cs`:

```csharp
// Add file upload services
builder.Services.AddFileUploadServices(builder.Configuration);

// Or with inline configuration
builder.Services.AddFileUploadServices(options =>
{
    options.BasePath = "Uploads";
    options.DefaultMaxSizeMB = 20;
});

var app = builder.Build();
app.Run();
```

## 4. Upload Files

### Backend (API Controller)

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
        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream);
        var fileData = memoryStream.ToArray();

        var result = await _fileUploadService.UploadAsync(
            fileData,
            file.FileName,
            UploadType.Document,
            file.ContentType);

        return result.IsSuccess 
            ? Ok(new { result.FilePath, result.FileId }) 
            : BadRequest(result.ErrorMessage);
    }
}
```

### Frontend (Blazor)

```razor
@inject IFileUploadService FileUploadService

<InputFile OnChange="HandleFileSelected" />
<p>Upload Progress: @_progress%</p>

@if (!string.IsNullOrEmpty(_message))
{
    <p>@_message</p>
}

@code {
    private int _progress;
    private string? _message;

    private async Task HandleFileSelected(InputFileChangeEventArgs e)
    {
        var result = await FileUploadService.UploadBrowserFileAsync(
            e.File,
            UploadType.ProfilePicture,
            progress => _progress = progress);

        _message = result.IsSuccess
            ? $"File uploaded successfully: {result.FilePath}"
            : $"Upload failed: {result.ErrorMessage}";
    }
}
```

## 5. Configuration Options

### Upload Types

Define allowed extensions and size limits per type:

```json
{
  "FileUploadOptions": {
    "UploadTypes": {
      "Image": {
        "MaxSizeMB": 5,
        "AllowedExtensions": [".jpg", ".jpeg", ".png", ".gif"],
        "Folder": "Images\\Assets"
      },
      "Document": {
        "MaxSizeMB": 10,
        "AllowedExtensions": [".pdf", ".doc", ".docx"],
        "Folder": "Documents"
      }
    }
  }
}
```

### Multi-Tenancy

Enable automatic tenant isolation:

```json
{
  "FileUploadOptions": {
    "EnableMultiTenancy": true
  }
}
```

Files will be stored in: `Files/Tenants/{TenantId}/Documents/file.pdf`

### Time-Limited Access Tokens

Enable secure file downloads:

```json
{
  "FileUploadOptions": {
    "UseTimeLimitedTokens": true,
    "TokenExpirationMinutes": 60
  }
}
```

## 6. Advanced Features

### Custom Storage Provider

```csharp
public class AzureBlobStorageProvider : IFileStorageProvider
{
    public string Name => "azure";
    
    public async Task<string> UploadAsync(Stream stream, string fileName, string folderPath, CancellationToken cancellationToken)
    {
        // Upload to Azure Blob Storage
        // Return the blob path
    }
    
    // Implement other methods...
}

// Register
builder.Services
    .AddFileUploadServices(builder.Configuration)
    .AddFileStorageProvider<AzureBlobStorageProvider>();
```

### Virus Scanning

```csharp
public class MyVirusScanner : IVirusScanner
{
    public async Task<bool> ScanAsync(Stream stream, string fileName, CancellationToken cancellationToken)
    {
        // Scan the file
        return true; // true if clean, false if virus detected
    }
}

// Enable and register
builder.Services
    .AddFileUploadServices(options => options.EnableVirusScanning = true)
    .AddVirusScanner<MyVirusScanner>();
```

### Thumbnail Generation

```csharp
public class MyThumbnailGenerator : IThumbnailGenerator
{
    public async Task<string?> GenerateAsync(Stream sourceStream, string thumbnailPath, int width, int height, CancellationToken cancellationToken)
    {
        // Generate thumbnail
        // Save to thumbnailPath
        return thumbnailPath;
    }
}

// Enable and register
builder.Services
    .AddFileUploadServices(options => options.EnableThumbnailGeneration = true)
    .AddThumbnailGenerator<MyThumbnailGenerator>();
```

## 7. File Metadata

Every successful upload returns metadata:

```csharp
var result = await _fileUploadService.UploadAsync(data, fileName, uploadType);

if (result.IsSuccess)
{
    Console.WriteLine($"File: {result.Metadata.FileName}");
    Console.WriteLine($"Size: {result.Metadata.SizeInBytes} bytes");
    Console.WriteLine($"Type: {result.Metadata.ContentType}");
    Console.WriteLine($"Uploaded By: {result.Metadata.UploadedBy}");
    Console.WriteLine($"Uploaded At: {result.Metadata.UploadedAt}");
    Console.WriteLine($"Tenant: {result.Metadata.TenantId}");
}
```

## 8. Error Handling

```csharp
var result = await _fileUploadService.UploadAsync(data, fileName, uploadType);

if (!result.IsSuccess)
{
    // Check error message
    if (result.ErrorMessage.Contains("not allowed"))
    {
        // Invalid file extension
    }
    else if (result.ErrorMessage.Contains("exceeds"))
    {
        // File too large
    }
}
```

## 9. Check if File Exists

```csharp
var exists = await _fileUploadService.FileExistsAsync("Documents/report.pdf");
```

## 10. Get Full Physical Path

```csharp
var fullPath = _fileUploadService.GetFullPath("Documents/report.pdf");
// Returns: "C:\MyApp\Files\Documents\report.pdf"
```

## Common Issues

### File extension not allowed
- Check `AllowedExtensions` in configuration
- Extensions must include the dot: `.pdf` not `pdf`

### File size exceeded
- Increase `MaxSizeMB` for the upload type
- Check server limits (`MaxRequestBodySize`)

### Multi-tenancy not working
- Ensure `EnableMultiTenancy = true`
- Verify tenant service is registered

## Need More Help?

See the [full documentation](./README.md) for detailed information on all features, advanced scenarios, and troubleshooting.
