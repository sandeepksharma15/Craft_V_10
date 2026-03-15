using System.ComponentModel.DataAnnotations;

namespace Craft.Security;

public class PasswordChangeRequest<TKey> : IPasswordChangeRequest<TKey>
{
    [Required]
    [DataType(DataType.Password)]
    public string? ConfirmNewPassword { get; set; } = default!;

    public TKey Id { get; set; } = default!;

    [Required]
    [DataType(DataType.Password)]
    public string? NewPassword { get; set; } = default!;

    [Required]
    [DataType(DataType.Password)]
    public string? Password { get; set; } = default!;

    // Token is used by the reset-password flow only; not required for change-password.
    public string? Token { get; set; }
}

public class PasswordChangeRequest : PasswordChangeRequest<KeyType>, IPasswordChangeRequest;

