using Craft.Auditing;
using Craft.Controllers.ErrorHandling;
using Craft.Core;
using Craft.QuerySpec;
using Craft.QuerySpec.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Craft.AppComponents.Auditing;

[Route("api/[controller]")]
[ApiController]
public class AuditTrailController(IRepository<AuditTrail, KeyType> repository,
    ILogger<EntityController<AuditTrail, AuditTrail, KeyType>> logger, IDatabaseErrorHandler databaseErrorHandler,
    IAuditTrailRepository auditTrailRepository, ILogger<AuditTrailController> auditTrailLogger)
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
            var tableNames = await auditTrailRepository.GetTableNamesAsync(cancellationToken);
            return Ok(tableNames);
        }
        catch (Exception ex)
        {
            auditTrailLogger.LogError(ex, "[AuditTrailController] Error retrieving table names");
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
            var users = await auditTrailRepository.GetAuditUsersAsync(cancellationToken);
            return Ok(users);
        }
        catch (Exception ex)
        {
            auditTrailLogger.LogError(ex, "[AuditTrailController] Error retrieving audit users");
            return BadRequest(new[] { "Failed to retrieve audit users" });
        }
    }
}

