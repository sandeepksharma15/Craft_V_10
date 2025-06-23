namespace Craft.Security;

public interface IPasswordChangeRequest<TKey>
{
    string? ConfirmNewPassword { get; set; }
    TKey Id { get; set; }
    string? NewPassword { get; set; }
    string? Password { get; set; }
    string? Token { get; set; }
}

public interface IPasswordChangeRequest : IPasswordChangeRequest<KeyType>;
