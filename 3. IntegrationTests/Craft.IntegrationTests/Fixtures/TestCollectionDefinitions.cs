namespace Craft.IntegrationTests.Fixtures;

/// <summary>
/// Collection definition for database integration tests.
/// Ensures tests share a single database instance.
/// </summary>
[CollectionDefinition(nameof(DatabaseTestCollection))]
public class DatabaseTestCollection : ICollectionFixture<DatabaseFixture>;

/// <summary>
/// Collection definition for in-memory database tests.
/// </summary>
[CollectionDefinition(nameof(InMemoryDatabaseTestCollection))]
public class InMemoryDatabaseTestCollection : ICollectionFixture<InMemoryDatabaseFixture>;
