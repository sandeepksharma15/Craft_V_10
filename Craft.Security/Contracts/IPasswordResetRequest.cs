namespace Craft.Security.Contracts;

public interface IPasswordResetRequest<TKey>
{
    string? Email { get; set; }
    TKey Id { get; set; }
    string? Password { get; set; }
    string? Token { get; set; }
}

public interface IPasswordResetRequest : IPasswordResetRequest<KeyType>;
