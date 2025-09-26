using Microsoft.EntityFrameworkCore;

namespace Craft.Data;

public class DbContextFeatureCollection : List<IDbContextFeature>
{
    public void ApplyConventions(ModelConfigurationBuilder builder)
    {
        foreach (var feature in this)
            feature.ConfigureConventions(builder);
    }

    public void ApplyModel(ModelBuilder builder)
    {
        foreach (var feature in this)
            feature.ConfigureModel(builder);
    }

    public void BeforeSaveChanges(DbContext context)
    {
        foreach (var feature in this)
            feature.OnBeforeSaveChanges(context);
    }
}
