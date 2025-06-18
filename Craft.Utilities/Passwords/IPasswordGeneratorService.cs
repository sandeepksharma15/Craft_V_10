namespace Craft.Utilities.Passwords;

public interface IPasswordGeneratorService
{
    string GeneratePassword(int length = 8);
}
