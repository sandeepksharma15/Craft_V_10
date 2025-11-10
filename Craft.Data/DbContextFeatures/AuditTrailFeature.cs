using Craft.Auditing;
using Craft.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Craft.Data.DbContextFeatures;

public class AuditTrailFeature : IDbContextFeature
{
    public void OnBeforeSaveChanges(DbContext context)
    {
        // Implement audit trail logic here
    }

    public void ConfigureModel(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AuditTrail>();
    }
}
