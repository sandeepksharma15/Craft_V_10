using Microsoft.Extensions.DependencyInjection;

namespace Craft.Data.Helpers;

public class CustomSeederRunner(IServiceProvider serviceProvider)
{
    private readonly ICustomSeeder[] _seeders = [.. serviceProvider.GetServices<ICustomSeeder>()];

    public async Task RunSeedersAsync(CancellationToken cancellationToken)
    {
        foreach (var seeder in _seeders)
            await seeder.InitializeAsync(cancellationToken);
    }
}
