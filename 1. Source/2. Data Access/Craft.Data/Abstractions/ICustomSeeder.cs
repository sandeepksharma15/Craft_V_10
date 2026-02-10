namespace Craft.Data;

public interface ICustomSeeder
{
    Task InitializeAsync(CancellationToken cancellationToken);
}

