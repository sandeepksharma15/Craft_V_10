using Microsoft.EntityFrameworkCore;

namespace Craft.Security;

public interface ILoginHistoryDbContext
{
    DbSet<LoginHistory> LoginHistories { get; set; }
}
