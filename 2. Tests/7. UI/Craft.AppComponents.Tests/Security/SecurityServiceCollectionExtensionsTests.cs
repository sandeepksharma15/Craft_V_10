using Craft.AppComponents.Security;
using Craft.Domain;
using Craft.QuerySpec;
using Craft.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Craft.AppComponents.Tests.Security;

/// <summary>
/// Unit tests for <see cref="ServiceCollectionExtensions"/> (Security) verifying DI registrations.
/// Uses ServiceDescriptor inspection to avoid requiring full infrastructure resolution.
/// </summary>
public class SecurityServiceCollectionExtensionsTests
{
    // ── Test stub types ─────────────────────────────────────────────────────────

    // Satisfies: CraftUser<KeyType>, new()
    private sealed class TestAppUser : CraftUser<KeyType> { }

    // Satisfies: IEntity, IModel, IEntity<KeyType>, IModel<KeyType>, new()
    private sealed class TestSecUser : IEntity, IModel
    {
        public KeyType Id { get; set; }
    }

    private sealed class TestSecRole : IEntity, IModel
    {
        public KeyType Id { get; set; }
    }

    private sealed class TestVM : IModel
    {
        public KeyType Id { get; set; }
    }

    private sealed class TestDTO : IModel
    {
        public KeyType Id { get; set; }
    }

    // Custom email sender used to verify the TryAdd behaviour
    private sealed class CustomEmailSender : IEmailSender<TestAppUser>
    {
        public Task SendConfirmationLinkAsync(TestAppUser user, string email, string confirmationLink) => Task.CompletedTask;
        public Task SendPasswordResetCodeAsync(TestAppUser user, string email, string resetCode) => Task.CompletedTask;
        public Task SendPasswordResetLinkAsync(TestAppUser user, string email, string resetLink) => Task.CompletedTask;
    }

    // ── AddAuthApi ──────────────────────────────────────────────────────────────

    [Fact]
    public void AddAuthApi_RegistersIAuthRepository()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddAuthApi<TestAppUser>();

        // Assert
        Assert.Contains(services, sd => sd.ServiceType == typeof(IAuthRepository));
    }

    [Fact]
    public void AddAuthApi_RegistersNoOpEmailSenderAsFallback()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddAuthApi<TestAppUser>();

        // Assert
        Assert.Contains(services, sd => sd.ServiceType == typeof(IEmailSender<TestAppUser>));
    }

    [Fact]
    public void AddAuthApi_DoesNotOverrideExistingEmailSender()
    {
        // Arrange – register a custom sender first
        var services = new ServiceCollection();
        services.AddScoped<IEmailSender<TestAppUser>, CustomEmailSender>();

        // Act – TryAdd inside AddAuthApi should be a no-op
        services.AddAuthApi<TestAppUser>();

        // Assert – only the explicitly registered CustomEmailSender remains
        var registrations = services
            .Where(sd => sd.ServiceType == typeof(IEmailSender<TestAppUser>))
            .ToList();

        Assert.Single(registrations);
        Assert.Equal(typeof(CustomEmailSender), registrations[0].ImplementationType);
    }

    // ── AddSecurityApi ──────────────────────────────────────────────────────────

    [Fact]
    public void AddSecurityApi_RegistersUsersRepository()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddSecurityApi<TestSecUser, TestSecRole>();

        // Assert
        Assert.Contains(services, sd => sd.ServiceType == typeof(IUsersRepository<TestSecUser, KeyType>));
    }

    [Fact]
    public void AddSecurityApi_RegistersRolesRepository()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddSecurityApi<TestSecUser, TestSecRole>();

        // Assert
        Assert.Contains(services, sd => sd.ServiceType == typeof(IRolesRepository<TestSecRole, KeyType>));
    }

    // ── AddAuthUI ───────────────────────────────────────────────────────────────

    [Fact]
    public void AddAuthUI_RegistersIAuthHttpService()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddAuthUI<TestVM>(sp => new HttpClient(), "https://api");

        // Assert
        Assert.Contains(services, sd => sd.ServiceType == typeof(IAuthHttpService<TestVM>));
    }

    [Fact]
    public void AddAuthUI_RegistersServiceAsTransient()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddAuthUI<TestVM>(sp => new HttpClient(), "https://api");

        // Assert
        var descriptor = services.Single(sd => sd.ServiceType == typeof(IAuthHttpService<TestVM>));
        Assert.Equal(ServiceLifetime.Transient, descriptor.Lifetime);
    }

    // ── AddSecurityUI ───────────────────────────────────────────────────────────

    [Fact]
    public void AddSecurityUI_RegistersIUsersHttpService()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddSecurityUI<TestSecUser, TestVM, TestDTO, TestSecRole, TestVM, TestDTO>(
            sp => new HttpClient(), "https://api");

        // Assert
        Assert.Contains(services, sd => sd.ServiceType == typeof(IUsersHttpService<TestSecUser, TestVM, TestDTO, KeyType>));
    }

    [Fact]
    public void AddSecurityUI_RegistersIRolesHttpService()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddSecurityUI<TestSecUser, TestVM, TestDTO, TestSecRole, TestVM, TestDTO>(
            sp => new HttpClient(), "https://api");

        // Assert
        Assert.Contains(services, sd => sd.ServiceType == typeof(IRolesHttpService<TestSecRole, TestVM, TestDTO, KeyType>));
    }

    [Fact]
    public void AddSecurityUI_RegistersSimplifiedIHttpServiceForUsers()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddSecurityUI<TestSecUser, TestVM, TestDTO, TestSecRole, TestVM, TestDTO>(
            sp => new HttpClient(), "https://api");

        // Assert – simplified IHttpService<TUser> (without key type) must also be registered
        Assert.Contains(services, sd => sd.ServiceType == typeof(IHttpService<TestSecUser>));
    }

    [Fact]
    public void AddSecurityUI_RegistersSimplifiedIHttpServiceForRoles()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddSecurityUI<TestSecUser, TestVM, TestDTO, TestSecRole, TestVM, TestDTO>(
            sp => new HttpClient(), "https://api");

        // Assert – simplified IHttpService<TRole> (without key type) must also be registered
        Assert.Contains(services, sd => sd.ServiceType == typeof(IHttpService<TestSecRole>));
    }
}
