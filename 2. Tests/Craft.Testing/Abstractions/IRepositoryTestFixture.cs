using Craft.Core;
using Microsoft.Extensions.DependencyInjection;

namespace Craft.Testing.Abstractions;

/// <summary>
/// Interface that defines the contract for repository test fixtures.
/// Fixtures implementing this interface can be used with BaseReadRepositoryTests and derived classes.
/// </summary>
public interface IRepositoryTestFixture
{
    /// <summary>
    /// Gets the database context for the test fixture.
    /// </summary>
    IDbContext DbContext { get; }

    /// <summary>
    /// Gets the service provider for resolving dependencies.
    /// </summary>
    IServiceProvider ServiceProvider { get; }

    /// <summary>
    /// Resets the database to a clean state.
    /// This is called before and after each test to ensure test isolation.
    /// </summary>
    Task ResetDatabaseAsync();
}
