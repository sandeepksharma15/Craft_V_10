using Microsoft.EntityFrameworkCore;

namespace Craft.Auditing;

public interface IHasAuditTrail
{
    DbSet<AuditTrail> AuditTrails { get; set; }
}
