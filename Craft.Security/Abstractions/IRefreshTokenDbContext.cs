using Microsoft.EntityFrameworkCore;

namespace Craft.Security;

public interface IRefreshTokenDbContext
{
    DbSet<RefreshToken> RefreshTokens { get; set; }
}
