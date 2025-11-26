using Craft.Infrastructure.Emails;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace Craft.Infrastructure.Tests.Emails;

public class EmailProviderFactoryTests
{
    [Fact]
    public void GetProvider_WithValidName_ReturnsProvider()
    {
        // Arrange
        var mockProvider = new Mock<IEmailProvider>();
        mockProvider.Setup(p => p.Name).Returns("test");

        var providers = new[] { mockProvider.Object };
        var options = Options.Create(new EmailOptions { Provider = "test" });
        var logger = new Mock<ILogger<EmailProviderFactory>>();

        var factory = new EmailProviderFactory(providers, options, logger.Object);

        // Act
        var provider = factory.GetProvider("test");

        // Assert
        Assert.NotNull(provider);
        Assert.Equal("test", provider.Name);
    }

    [Fact]
    public void GetProvider_WithInvalidName_ThrowsException()
    {
        // Arrange
        var providers = Array.Empty<IEmailProvider>();
        var options = Options.Create(new EmailOptions { Provider = "test" });
        var logger = new Mock<ILogger<EmailProviderFactory>>();

        var factory = new EmailProviderFactory(providers, options, logger.Object);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => factory.GetProvider("nonexistent"));
    }

    [Fact]
    public void GetProvider_IsCaseInsensitive()
    {
        // Arrange
        var mockProvider = new Mock<IEmailProvider>();
        mockProvider.Setup(p => p.Name).Returns("smtp");

        var providers = new[] { mockProvider.Object };
        var options = Options.Create(new EmailOptions { Provider = "smtp" });
        var logger = new Mock<ILogger<EmailProviderFactory>>();

        var factory = new EmailProviderFactory(providers, options, logger.Object);

        // Act
        var provider1 = factory.GetProvider("smtp");
        var provider2 = factory.GetProvider("SMTP");
        var provider3 = factory.GetProvider("Smtp");

        // Assert
        Assert.NotNull(provider1);
        Assert.NotNull(provider2);
        Assert.NotNull(provider3);
    }

    [Fact]
    public void GetDefaultProvider_ReturnsConfiguredProvider()
    {
        // Arrange
        var smtpProvider = new Mock<IEmailProvider>();
        smtpProvider.Setup(p => p.Name).Returns("smtp");

        var mockProvider = new Mock<IEmailProvider>();
        mockProvider.Setup(p => p.Name).Returns("mock");

        var providers = new[] { smtpProvider.Object, mockProvider.Object };
        var options = Options.Create(new EmailOptions { Provider = "mock" });
        var logger = new Mock<ILogger<EmailProviderFactory>>();

        var factory = new EmailProviderFactory(providers, options, logger.Object);

        // Act
        var provider = factory.GetDefaultProvider();

        // Assert
        Assert.NotNull(provider);
        Assert.Equal("mock", provider.Name);
    }

    [Fact]
    public void GetAllProviders_ReturnsAllRegisteredProviders()
    {
        // Arrange
        var provider1 = new Mock<IEmailProvider>();
        provider1.Setup(p => p.Name).Returns("smtp");

        var provider2 = new Mock<IEmailProvider>();
        provider2.Setup(p => p.Name).Returns("mock");

        var providers = new[] { provider1.Object, provider2.Object };
        var options = Options.Create(new EmailOptions { Provider = "smtp" });
        var logger = new Mock<ILogger<EmailProviderFactory>>();

        var factory = new EmailProviderFactory(providers, options, logger.Object);

        // Act
        var allProviders = factory.GetAllProviders().ToList();

        // Assert
        Assert.Equal(2, allProviders.Count);
        Assert.Contains(allProviders, p => p.Name == "smtp");
        Assert.Contains(allProviders, p => p.Name == "mock");
    }
}
