using System.Text.Json;
using Microsoft.AspNetCore.Identity;

namespace Craft.Core;

public class ServerResponse : IServiceResult
{
    public object? Data { get; set; }
    public bool IsSuccess { get; set; }
    public List<string> Errors { get; set; } = [];
    public int StatusCode { get; set; }

    public string? ErrorId { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string? Message { get; set; }
    public string? Source { get; set; }
    public string? SupportMessage { get; set; }

    // IServiceResult implementation
    int? IServiceResult.StatusCode => StatusCode;
    public bool HasErrors => Errors.Count > 0;
    public bool IsFailure => !IsSuccess;

    public ServerResponse() { }

    public ServerResponse(IdentityResult identityResult)
    {
        IsSuccess = identityResult.Succeeded;
        Errors = [.. identityResult.Errors.Select(x => x.Description)];
        Data = null;
    }

    public override string ToString() => JsonSerializer.Serialize(this);
}
