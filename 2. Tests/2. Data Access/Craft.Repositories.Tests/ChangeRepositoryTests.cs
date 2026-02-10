using Craft.Testing.Fixtures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Craft.Repositories.Tests;

[Collection(nameof(SystemTestCollectionDefinition))]
public class ChangeRepositoryTests
{
    private static DbContextOptions<TestDbContext> CreateOptions() =>
        new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

    private static ChangeRepository<Country, KeyType> CreateRepository(TestDbContext context)
    {
        var logger = new Logger<ChangeRepository<Country, KeyType>>(new LoggerFactory());
        return new ChangeRepository<Country, KeyType>(context, logger);
    }

    [Fact]
    public async Task AddAsync_AddsEntity_WhenValid()
    {
        // Arrange
        var options = CreateOptions();
        await using var context = new TestDbContext(options);
        context.Database.EnsureCreated();
        var repo = CreateRepository(context);
        var country = new Country { Name = "TestCountry" };

        // Act
        var result = await repo.AddAsync(country);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("TestCountry", result.Name);
        Assert.True(result.Id > 0);
        Assert.Contains(context.Countries!, c => c.Name == "TestCountry");
    }

    [Fact]
    public async Task AddAsync_ThrowsArgumentNullException_WhenEntityIsNull()
    {
        // Arrange
        var options = CreateOptions();
        await using var context = new TestDbContext(options);
        var repo = CreateRepository(context);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => repo.AddAsync(null!));
    }

    [Fact]
    public async Task AddAsync_RespectsCancellationToken()
    {
        // Arrange
        var options = CreateOptions();
        await using var context = new TestDbContext(options);
        var repo = CreateRepository(context);
        var country = new Country { Name = "TestCountry" };
        using var cts = new CancellationTokenSource();

        // Act & Assert
        cts.Cancel();
        await Assert.ThrowsAsync<TaskCanceledException>(() => repo.AddAsync(country, cancellationToken: cts.Token));
    }

    [Fact]
    public async Task AddAsync_DoesNotSave_WhenAutoSaveIsFalse()
    {
        // Arrange
        var options = CreateOptions();
        await using var context = new TestDbContext(options);
        var repo = CreateRepository(context);
        var country = new Country { Name = "TestCountry" };

        // Act
        var result = await repo.AddAsync(country, autoSave: false);

        // Assert
        Assert.NotNull(result);
        Assert.DoesNotContain(context.Countries!, c => c.Name == "TestCountry");
    }

    [Fact]
    public async Task AddRangeAsync_AddsEntities_WhenValid()
    {
        // Arrange
        var options = CreateOptions();
        await using var context = new TestDbContext(options);
        var repo = CreateRepository(context);
        var countries = new List<Country> {
            new() { Name = "Country1" },
            new() { Name = "Country2" }
        };

        // Act
        var result = await repo.AddRangeAsync(countries);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(context.Countries!, c => c.Name == "Country1");
        Assert.Contains(context.Countries!, c => c.Name == "Country2");
    }

    [Fact]
    public async Task AddRangeAsync_ThrowsArgumentNullException_WhenEntitiesIsNull()
    {
        // Arrange
        var options = CreateOptions();
        await using var context = new TestDbContext(options);
        var repo = CreateRepository(context);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => repo.AddRangeAsync(null!));
    }

    [Fact]
    public async Task AddRangeAsync_RespectsCancellationToken()
    {
        // Arrange
        var options = CreateOptions();
        await using var context = new TestDbContext(options);
        var repo = CreateRepository(context);
        var countries = new List<Country> { new() { Name = "Country1" } };
        using var cts = new CancellationTokenSource();

        // Act & Assert
        cts.Cancel();
        await Assert.ThrowsAsync<TaskCanceledException>(() => repo.AddRangeAsync(countries, cancellationToken: cts.Token));
    }

    [Fact]
    public async Task AddRangeAsync_DoesNotSave_WhenAutoSaveIsFalse()
    {
        // Arrange
        var options = CreateOptions();
        await using var context = new TestDbContext(options);
        var repo = CreateRepository(context);
        var countries = new List<Country> { new() { Name = "Country1" } };

        // Act
        var result = await repo.AddRangeAsync(countries, autoSave: false);

        // Assert
        Assert.NotNull(result);
        Assert.DoesNotContain(context.Countries!, c => c.Name == "Country1");
    }

    [Fact]
    public async Task UpdateAsync_UpdatesEntity_WhenValid()
    {
        // Arrange
        var options = CreateOptions();
        await using var context = new TestDbContext(options);
        context.Database.EnsureCreated();
        var repo = CreateRepository(context);
        var country = new Country { Name = "OldName" };

        // Act
        await repo.AddAsync(country);
        country.Name = "NewName";
        var result = await repo.UpdateAsync(country);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("NewName", result.Name);
        Assert.Contains(context.Countries!, c => c.Name == "NewName");
    }

    [Fact]
    public async Task UpdateAsync_ThrowsArgumentNullException_WhenEntityIsNull()
    {
        // Arrange
        var options = CreateOptions();
        await using var context = new TestDbContext(options);
        var repo = CreateRepository(context);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => repo.UpdateAsync(null!));
    }

    [Fact]
    public async Task UpdateAsync_RespectsCancellationToken()
    {
        // Arrange
        var options = CreateOptions();
        await using var context = new TestDbContext(options);
        var repo = CreateRepository(context);
        var country = new Country { Name = "TestCountry" };
        await repo.AddAsync(country);
        using var cts = new CancellationTokenSource();

        // Act & Assert
        cts.Cancel();
        await Assert.ThrowsAsync<TaskCanceledException>(() => repo.UpdateAsync(country, cancellationToken: cts.Token));
    }

    [Fact]
    public async Task UpdateAsync_DoesNotSave_WhenAutoSaveIsFalse()
    {
        // Arrange
        var options = CreateOptions();
        await using var context = new TestDbContext(options);
        var repo = CreateRepository(context);
        var country = new Country { Name = "OldName" };

        // Act
        await repo.AddAsync(country);
        country.Name = "NewName";
        var result = await repo.UpdateAsync(country, autoSave: false);

        // Assert
        Assert.NotNull(result);

        // Verify using a fresh context that the update wasn't persisted
        await using var verifyContext = new TestDbContext(options);
        Assert.DoesNotContain(verifyContext.Countries!, c => c.Name == "NewName");
    }

    [Fact]
    public async Task UpdateRangeAsync_UpdatesEntities_WhenValid()
    {
        // Arrange
        var options = CreateOptions();
        await using var context = new TestDbContext(options);
        var repo = CreateRepository(context);
        var countries = new List<Country> {
            new() { Name = "Country1" },
            new() { Name = "Country2" }
        };

        // Act
        await repo.AddRangeAsync(countries);
        countries[0].Name = "Updated1";
        countries[1].Name = "Updated2";
        var result = await repo.UpdateRangeAsync(countries);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(context.Countries!, c => c.Name == "Updated1");
        Assert.Contains(context.Countries!, c => c.Name == "Updated2");
    }

    [Fact]
    public async Task UpdateRangeAsync_ThrowsArgumentNullException_WhenEntitiesIsNull()
    {
        // Arrange
        var options = CreateOptions();
        await using var context = new TestDbContext(options);
        var repo = CreateRepository(context);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => repo.UpdateRangeAsync(null!));
    }

    [Fact]
    public async Task UpdateRangeAsync_RespectsCancellationToken()
    {
        // Arrange
        var options = CreateOptions();
        await using var context = new TestDbContext(options);
        var repo = CreateRepository(context);
        var countries = new List<Country> { new() { Name = "Country1" } };
        await repo.AddRangeAsync(countries);
        using var cts = new CancellationTokenSource();

        // Act & Assert
        cts.Cancel();
        await Assert.ThrowsAsync<TaskCanceledException>(() => repo.UpdateRangeAsync(countries, cancellationToken: cts.Token));
    }

    [Fact]
    public async Task UpdateRangeAsync_DoesNotSave_WhenAutoSaveIsFalse()
    {
        // Arrange
        var options = CreateOptions();
        await using var context = new TestDbContext(options);
        var repo = CreateRepository(context);
        var countries = new List<Country> { new() { Name = "Country1" } };

        // Act
        await repo.AddRangeAsync(countries);
        countries[0].Name = "Updated1";
        var result = await repo.UpdateRangeAsync(countries, autoSave: false);

        // Assert
        Assert.NotNull(result);

        // Verify using a fresh context that the update wasn't persisted
        await using var verifyContext = new TestDbContext(options);
        Assert.DoesNotContain(verifyContext.Countries!, c => c.Name == "Updated1");
    }

    [Fact]
    public async Task DeleteAsync_DeletesEntity_WhenNotSoftDelete()
    {
        // Arrange
        var options = CreateOptions();
        await using var context = new TestDbContext(options);
        var logger = new Logger<ChangeRepository<NoSoftDeleteEntity, KeyType>>(new LoggerFactory());
        var repo = new ChangeRepository<NoSoftDeleteEntity, KeyType>(context, logger);

        var entity = new NoSoftDeleteEntity { Desc = "ToDelete" };

        // Act
        await repo.AddAsync(entity);
        var result = await repo.DeleteAsync(entity);

        // Assert
        Assert.NotNull(result);
        Assert.DoesNotContain(context.NoSoftDeleteEntities!, c => c.Desc == "ToDelete");
    }

    [Fact]
    public async Task DeleteAsync_ThrowsArgumentNullException_WhenEntityIsNull()
    {
        // Arrange
        var options = CreateOptions();
        await using var context = new TestDbContext(options);
        var repo = CreateRepository(context);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => repo.DeleteAsync(null!));
    }

    [Fact]
    public async Task DeleteAsync_RespectsCancellationToken()
    {
        // Arrange
        var options = CreateOptions();
        await using var context = new TestDbContext(options);
        var repo = CreateRepository(context);
        var country = new Country { Name = "ToDelete" };
        await repo.AddAsync(country);
        using var cts = new CancellationTokenSource();

        // Act & Assert
        cts.Cancel();
        await Assert.ThrowsAsync<TaskCanceledException>(() => repo.DeleteAsync(country, cancellationToken: cts.Token));
    }

    [Fact]
    public async Task DeleteAsync_DoesNotSave_WhenAutoSaveIsFalse()
    {
        // Arrange
        var options = CreateOptions();
        await using var context = new TestDbContext(options);
        var repo = CreateRepository(context);
        var country = new Country { Name = "ToDelete" };

        // Act
        await repo.AddAsync(country);
        var result = await repo.DeleteAsync(country, autoSave: false);

        // Assert
        Assert.NotNull(result);
        Assert.Contains(context.Countries!, c => c.Name == "ToDelete");
    }

    [Fact]
    public async Task DeleteRangeAsync_DeletesEntities_WhenNotSoftDelete()
    {
        // Arrange
        var options = CreateOptions();
        await using var context = new TestDbContext(options);

        var logger = new Logger<ChangeRepository<NoSoftDeleteEntity, KeyType>>(new LoggerFactory());
        var repo = new ChangeRepository<NoSoftDeleteEntity, KeyType>(context, logger);

        var entity = new NoSoftDeleteEntity { Desc = "ToDelete" };

        var countries = new List<NoSoftDeleteEntity> {
            new() { Desc = "ToDelete1" },
            new() { Desc = "ToDelete2" }
        };

        // Act
        await repo.AddRangeAsync(countries);
        var result = await repo.DeleteRangeAsync(countries);

        // Assert
        Assert.Equal(2, result?.Count);
        Assert.DoesNotContain(context.NoSoftDeleteEntities!, c => c.Desc == "ToDelete1");
        Assert.DoesNotContain(context.NoSoftDeleteEntities!, c => c.Desc == "ToDelete2");
    }

    [Fact]
    public async Task DeleteRangeAsync_ThrowsArgumentNullException_WhenEntitiesIsNull()
    {
        // Arrange
        var options = CreateOptions();
        await using var context = new TestDbContext(options);
        var repo = CreateRepository(context);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => repo.DeleteRangeAsync(null!));
    }

    [Fact]
    public async Task DeleteRangeAsync_RespectsCancellationToken()
    {
        // Arrange
        var options = CreateOptions();
        await using var context = new TestDbContext(options);
        var repo = CreateRepository(context);
        var countries = new List<Country> { new() { Name = "ToDelete1" } };
        await repo.AddRangeAsync(countries);
        using var cts = new CancellationTokenSource();

        // Act & Assert
        cts.Cancel();
        await Assert.ThrowsAsync<TaskCanceledException>(() => repo.DeleteRangeAsync(countries, cancellationToken: cts.Token));
    }

    [Fact]
    public async Task DeleteRangeAsync_DoesNotSave_WhenAutoSaveIsFalse()
    {
        // Arrange
        var options = CreateOptions();
        await using var context = new TestDbContext(options);
        var repo = CreateRepository(context);
        var countries = new List<Country> { new() { Name = "ToDelete1" } };

        // Act
        await repo.AddRangeAsync(countries);
        var result = await repo.DeleteRangeAsync(countries, autoSave: false);

        // Assert
        Assert.NotNull(result);
        Assert.Contains(context.Countries!, c => c.Name == "ToDelete1");
    }

    [Fact]
    public async Task DeleteAsync_SoftDeletesEntity_WhenImplementsISoftDelete()
    {
        // Arrange
        var options = CreateOptions();
        await using var context = new TestDbContext(options);
        var logger = new Logger<ChangeRepository<Country, KeyType>>(new LoggerFactory());
        var repo = new ChangeRepository<Country, KeyType>(context, logger);
        var country = new Country { Name = "SoftDelete" };

        // Act
        await repo.AddAsync(country);
        var result = await repo.DeleteAsync(country);

        // Assert
        Assert.NotNull(result);
        Assert.True(country.IsDeleted);
        Assert.Contains(context.Countries!.OfType<Country>(), c => c.Name == "SoftDelete" && c.IsDeleted);
    }

    [Fact]
    public async Task DeleteRangeAsync_SoftDeletesEntities_WhenImplementsISoftDelete()
    {
        // Arrange
        var options = CreateOptions();
        await using var context = new TestDbContext(options);
        var logger = new Logger<ChangeRepository<Country, KeyType>>(new LoggerFactory());
        var repo = new ChangeRepository<Country, KeyType>(context, logger);
        var countries = new List<Country> {
            new() { Name = "SoftDelete1" },
            new() { Name = "SoftDelete2" }
        };

        // Act
        await repo.AddRangeAsync(countries);
        var result = await repo.DeleteRangeAsync(countries);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(countries, c => Assert.True(c.IsDeleted));
        Assert.Contains(context.Countries!.OfType<Country>(), c => c.Name == "SoftDelete1" && c.IsDeleted);
        Assert.Contains(context.Countries!.OfType<Country>(), c => c.Name == "SoftDelete2" && c.IsDeleted);
    }

    [Fact]
    public async Task DeleteRangeAsync_HandlesMixedEntities_WhenSomeImplementISoftDelete()
    {
        // Arrange
        var options = CreateOptions();
        await using var context = new TestDbContext(options);
        context.Database.EnsureCreated();

        var softDeleteLogger = new Logger<ChangeRepository<Country, KeyType>>(new LoggerFactory());
        var softDeleteRepo = new ChangeRepository<Country, KeyType>(context, softDeleteLogger);

        var hardDeleteLogger = new Logger<ChangeRepository<NoSoftDeleteEntity, KeyType>>(new LoggerFactory());
        var hardDeleteRepo = new ChangeRepository<NoSoftDeleteEntity, KeyType>(context, hardDeleteLogger);

        var softDeleteCountries = new List<Country> {
            new() { Name = "SoftDelete1" },
            new() { Name = "SoftDelete2" }
        };

        var hardDeleteEntities = new List<NoSoftDeleteEntity> {
            new() { Desc = "HardDelete1" },
            new() { Desc = "HardDelete2" }
        };

        // Act
        await softDeleteRepo.AddRangeAsync(softDeleteCountries);
        await hardDeleteRepo.AddRangeAsync(hardDeleteEntities);

        await softDeleteRepo.DeleteRangeAsync(softDeleteCountries);
        await hardDeleteRepo.DeleteRangeAsync(hardDeleteEntities);

        // Assert - soft delete entities should still exist but marked deleted
        Assert.All(softDeleteCountries, c => Assert.True(c.IsDeleted));
        Assert.Contains(context.Countries!, c => c.Name == "SoftDelete1" && c.IsDeleted);
        Assert.Contains(context.Countries!, c => c.Name == "SoftDelete2" && c.IsDeleted);

        // Assert - hard delete entities should be removed
        Assert.DoesNotContain(context.NoSoftDeleteEntities!, e => e.Desc == "HardDelete1");
        Assert.DoesNotContain(context.NoSoftDeleteEntities!, e => e.Desc == "HardDelete2");
    }

    [Fact]
    public async Task DeleteRangeAsync_HandlesEmptyList()
    {
        // Arrange
        var options = CreateOptions();
        await using var context = new TestDbContext(options);
        var repo = CreateRepository(context);
        var emptyList = new List<Country>();

        // Act
        var result = await repo.DeleteRangeAsync(emptyList);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task AddAsync_DetachesEntityAfterSave()
    {
        // Arrange
        var options = CreateOptions();
        await using var context = new TestDbContext(options);
        var repo = CreateRepository(context);
        var country = new Country { Name = "TestCountry" };

        // Act
        var result = await repo.AddAsync(country);

        // Assert
        Assert.Equal(EntityState.Detached, context.Entry(result).State);
    }

    [Fact]
    public async Task UpdateAsync_DetachesEntityAfterSave()
    {
        // Arrange
        var options = CreateOptions();
        await using var context = new TestDbContext(options);
        var repo = CreateRepository(context);
        var country = new Country { Name = "OldName" };
        await repo.AddAsync(country);
        country.Name = "NewName";

        // Act
        var result = await repo.UpdateAsync(country);

        // Assert
        Assert.Equal(EntityState.Detached, context.Entry(result).State);
    }

    [Fact]
    public async Task DeleteAsync_DetachesEntityAfterSave()
    {
        // Arrange
        var options = CreateOptions();
        await using var context = new TestDbContext(options);
        var repo = CreateRepository(context);
        var country = new Country { Name = "ToDelete" };
        await repo.AddAsync(country);

        // Act
        var result = await repo.DeleteAsync(country);

        // Assert
        Assert.Equal(EntityState.Detached, context.Entry(result).State);
    }

    [Fact]
    public async Task AddRangeAsync_DetachesAllEntitiesAfterSave()
    {
        // Arrange
        var options = CreateOptions();
        await using var context = new TestDbContext(options);
        var repo = CreateRepository(context);
        var countries = new List<Country> {
            new() { Name = "Country1" },
            new() { Name = "Country2" }
        };

        // Act
        var result = await repo.AddRangeAsync(countries);

        // Assert
        Assert.All(result, entity => Assert.Equal(EntityState.Detached, context.Entry(entity).State));
    }

    [Fact]
    public async Task UpdateRangeAsync_DetachesAllEntitiesAfterSave()
    {
        // Arrange
        var options = CreateOptions();
        await using var context = new TestDbContext(options);
        var repo = CreateRepository(context);
        var countries = new List<Country> {
            new() { Name = "Country1" },
            new() { Name = "Country2" }
        };
        await repo.AddRangeAsync(countries);
        countries.ForEach(c => c.Name = $"Updated{c.Name}");

        // Act
        var result = await repo.UpdateRangeAsync(countries);

        // Assert
        Assert.All(result, entity => Assert.Equal(EntityState.Detached, context.Entry(entity).State));
    }

    [Fact]
    public async Task DeleteRangeAsync_DetachesAllEntitiesAfterSave()
    {
        // Arrange
        var options = CreateOptions();
        await using var context = new TestDbContext(options);
        var repo = CreateRepository(context);
        var countries = new List<Country> {
            new() { Name = "ToDelete1" },
            new() { Name = "ToDelete2" }
        };
        await repo.AddRangeAsync(countries);

        // Act
        var result = await repo.DeleteRangeAsync(countries);

        // Assert
        Assert.All(result, entity => Assert.Equal(EntityState.Detached, context.Entry(entity).State));
    }
}

