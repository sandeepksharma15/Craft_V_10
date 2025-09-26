using Microsoft.EntityFrameworkCore;

namespace Craft.Data;

public interface IDbContextFeature
{
    void ConfigureConventions(ModelConfigurationBuilder configurationBuilder) { }
    void ConfigureModel(ModelBuilder modelBuilder) { }
    void OnBeforeSaveChanges(DbContext context) { }
    Task OnBeforeSaveChangesAsync(DbContext context) => Task.CompletedTask;
}
