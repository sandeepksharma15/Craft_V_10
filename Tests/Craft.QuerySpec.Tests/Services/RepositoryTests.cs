using Craft.TestDataStore.Fixtures;
using Craft.TestDataStore.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Craft.QuerySpec.Tests.Services;

public class RepositoryTests
{
    private static DbContextOptions<TestDbContext> CreateOptions() =>
        new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

    private static Repository<Country, KeyType> CreateRepository(TestDbContext context)
    {
        var logger = new Logger<Repository<Country, KeyType>>(new LoggerFactory());
        return new Repository<Country, KeyType>(context, logger);
    }

}
