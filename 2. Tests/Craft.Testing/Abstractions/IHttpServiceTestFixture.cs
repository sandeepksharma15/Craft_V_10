using Craft.Core;
using Microsoft.Extensions.DependencyInjection;

namespace Craft.Testing.Abstractions;

/// <summary>
/// Interface that defines the contract for HTTP service test fixtures.
/// Fixtures implementing this interface can be used with BaseReadHttpServiceTests and derived classes.
/// Combines API backend (DbContext) with HTTP client for end-to-end HTTP service testing.
/// </summary>
public interface IHttpServiceTestFixture
{
    /// <summary>
    /// Gets the database context for seeding test data on the API backend.
    /// </summary>
    IDbContext DbContext { get; }

    /// <summary>
    /// Gets the service provider for resolving dependencies.
    /// </summary>
    IServiceProvider ServiceProvider { get; }

    /// <summary>
    /// Gets the HTTP client configured to communicate with the test API.
    /// </summary>
    HttpClient HttpClient { get; }

    /// <summary>
    /// Gets the base URI for the API endpoints.
    /// </summary>
    Uri ApiBaseUri { get; }

    /// <summary>
    /// Resets the database to a clean state.
    /// This is called before and after each test to ensure test isolation.
    /// </summary>
    Task ResetDatabaseAsync();
}
