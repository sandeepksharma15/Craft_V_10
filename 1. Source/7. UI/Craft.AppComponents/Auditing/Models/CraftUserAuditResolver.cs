using Craft.Core;
using Craft.Security;
using Microsoft.EntityFrameworkCore;

namespace Craft.AppComponents.Auditing;

public class CraftUserAuditResolver<TUser>(IDbContext dbContext) : IAuditUserResolver
    where TUser : class, ICraftUser
{
    public async Task<List<AuditUserDTO>> GetAuditUsersAsync(List<KeyType> userIds, CancellationToken cancellationToken = default)
    {
        var users = await dbContext.Set<TUser>()
            .Where(u => userIds.Contains(u.Id))
            .Select(u => new { u.Id, u.FirstName, u.LastName, u.UserName })
            .ToListAsync(cancellationToken);

        return [.. users.Select(u =>
        {
            var fullName = $"{u.FirstName} {u.LastName}".Trim();
            return new AuditUserDTO(u.Id, fullName.Length > 0 ? fullName : u.UserName ?? u.Id.ToString());
        })];
    }
}
