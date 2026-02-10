namespace Craft.Data;

public interface IAppDbSetup
{
    Task SetupAppDbAsync(CancellationToken cancellationToken);
}

