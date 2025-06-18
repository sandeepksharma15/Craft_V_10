using System;

namespace Craft.Utilities.Passwords;

public class PasswordGeneratorService : IPasswordGeneratorService
{
    public string GeneratePassword(int length = 8)
        => PasswordGenerator.GeneratePassword(length);
}
