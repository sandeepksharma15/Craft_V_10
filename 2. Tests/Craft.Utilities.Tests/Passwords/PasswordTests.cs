using Craft.Utilities.Passwords;
using Microsoft.Extensions.DependencyInjection;

namespace Craft.Utilities.Tests.Passwords;

public class PasswordTests
{
    [Fact]
    public void PasswordGeneratorService_GeneratesPassword_DefaultLength()
    {
        // Arrange & Act
        var service = new PasswordGeneratorService();
        var password = service.GeneratePassword();

        // Assert
        Assert.Equal(8, password.Length);
    }

    [Fact]
    public void PasswordGeneratorService_GeneratesPassword_CustomLength()
    {
        // Arrange & Act
        var service = new PasswordGeneratorService();
        var password = service.GeneratePassword(12);

        // Assert
        Assert.Equal(12, password.Length);
    }

    [Fact]
    public void PasswordGeneratorService_Throws_OnTooShortLength()
    {
        // Arrange & Act & Assert
        var service = new PasswordGeneratorService();
        Assert.Throws<ArgumentOutOfRangeException>(() => service.GeneratePassword(5));
    }

    [Fact]
    public void PasswordGeneratorService_GeneratesPassword_WithAllCharacterTypes()
    {
        // Arrange & Act
        var service = new PasswordGeneratorService();
        var password = service.GeneratePassword(12);

        // Assert
        Assert.Contains(password, char.IsUpper);
        Assert.Contains(password, char.IsLower);
        Assert.Contains(password, char.IsDigit);
        Assert.Contains(password, c => "!@#$%^&*()_+[]{}|;:,.<>?".Contains(c));
    }

    [Fact]
    public void PasswordGeneratorService_GeneratesRandomPasswords()
    {
        // Arrange & Act & Assert
        var service = new PasswordGeneratorService();
        var p1 = service.GeneratePassword();
        var p2 = service.GeneratePassword();
        Assert.NotEqual(p1, p2);
    }

    [Fact]
    public void PasswordGenerator_Static_GeneratesPassword_DefaultLength()
    {
        // Arrange & Act
        var password = PasswordGenerator.GeneratePassword();

        // Assert
        Assert.Equal(8, password.Length);
    }

    [Fact]
    public void PasswordGenerator_Static_GeneratesPassword_CustomLength()
    {
        // Arrange & Act
        var password = PasswordGenerator.GeneratePassword(14);

        // Assert
        Assert.Equal(14, password.Length);
    }

    [Fact]
    public void PasswordGenerator_Static_Throws_OnTooShortLength()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => PasswordGenerator.GeneratePassword(3));
    }

    [Fact]
    public void PasswordGenerator_Static_GeneratesPassword_WithAllCharacterTypes()
    {
        // Arrange & Act
        var password = PasswordGenerator.GeneratePassword(10);

        // Assert
        Assert.Contains(password, char.IsUpper);
        Assert.Contains(password, char.IsLower);
        Assert.Contains(password, char.IsDigit);
        Assert.Contains(password, c => "!@#$%^&*()_+[]{}|;:,.<>?".Contains(c));
    }

    [Fact]
    public void PasswordGenerator_Static_GeneratesRandomPasswords()
    {
        // Arrange & Act & Assert
        var p1 = PasswordGenerator.GeneratePassword();
        var p2 = PasswordGenerator.GeneratePassword();
        Assert.NotEqual(p1, p2);
    }

    [Fact]
    public void PasswordGeneratorServiceExtensions_RegistersService()
    {
        // Arrange & Act
        var services = new ServiceCollection();
        services.AddPasswordGeneratorService();
        var provider = services.BuildServiceProvider();
        var resolved = provider.GetService<IPasswordGeneratorService>();

        // Assert
        Assert.NotNull(resolved);
        Assert.IsType<PasswordGeneratorService>(resolved);
    }

    [Fact]
    public void PasswordGeneratorServiceExtensions_RegistersService_AsSingleton()
    {
        // Arrange & Act
        var services = new ServiceCollection();
        services.AddPasswordGeneratorService();
        var provider = services.BuildServiceProvider();
        var s1 = provider.GetService<IPasswordGeneratorService>();
        var s2 = provider.GetService<IPasswordGeneratorService>();

        // Assert
        Assert.Same(s1, s2);
    }
}
