using Microsoft.EntityFrameworkCore;

namespace Craft.Auditing;

public interface IAuditTrailDbContext
{
    DbSet<AuditTrail> AuditTrails { get; set; }
}
