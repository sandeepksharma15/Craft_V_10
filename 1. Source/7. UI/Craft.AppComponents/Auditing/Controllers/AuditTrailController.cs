using Craft.Auditing;
using Craft.Controllers.ErrorHandling;
using Craft.Core;
using Craft.QuerySpec;
using Craft.QuerySpec.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using GccPT.Shared.Models.Authentication;

namespace Craft.AppComponents.Auditing;

[Route("api/[controller]")]
[ApiController]
public class AuditTrailController(IRepository<AuditTrail, KeyType> repository, ILogger<EntityController<AuditTrail, AuditTrail, KeyType>> logger,
    IDatabaseErrorHandler databaseErrorHandler, IDbContext dbContext)
    : EntityController<AuditTrail, AuditTrail, KeyType>(repository, logger, databaseErrorHandler)
{
    /// <summary>
    /// Returns the distinct table names that have audit trail entries.
    /// </summary>
    [HttpGet("tablenames")]
    [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<string>>> GetTableNamesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var tableNames = await dbContext.Set<AuditTrail>()
                .Where(x => !x.IsDeleted)
                .Select(x => x.TableName!)
                .Distinct()
                .OrderBy(x => x)
                .ToListAsync(cancellationToken);

            return Ok(tableNames);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[AuditTrailController] Error retrieving table names");
            return BadRequest(new[] { "Failed to retrieve table names" });
        }
    }

    /// <summary>
    /// Returns the users who have audit trail entries with their display names.
    /// </summary>
    [HttpGet("auditusers")]
    [ProducesResponseType(typeof(List<AuditUserDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<AuditUserDTO>>> GetAuditUsersAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var userIds = await dbContext.Set<AuditTrail>()
                .Where(x => !x.IsDeleted)
                .Select(x => x.UserId)
                .Distinct()
                .ToListAsync(cancellationToken);

            var users = await dbContext.Set<AppUser>()
                .Where(u => userIds.Contains(u.Id))
                .Select(u => new AuditUserDTO(u.Id, u.UserName ?? u.Email ?? u.Id.ToString()))
                .ToListAsync(cancellationToken);

            return Ok(users);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[AuditTrailController] Error retrieving audit users");
            return BadRequest(new[] { "Failed to retrieve audit users" });
        }
    }

}
