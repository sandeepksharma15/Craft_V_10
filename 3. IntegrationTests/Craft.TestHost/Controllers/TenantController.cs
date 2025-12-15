using Craft.MultiTenant;
using Microsoft.AspNetCore.Mvc;

namespace Craft.TestHost.Controllers;

/// <summary>
/// Controller for testing multi-tenant functionality in HTTP pipeline.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class TenantController : ControllerBase
{
    private readonly ITenantContextAccessor? _contextAccessor;

    public TenantController(ITenantContextAccessor? contextAccessor = null)
    {
        _contextAccessor = contextAccessor;
    }

    [HttpGet]
    public ActionResult<TenantInfo> GetCurrentTenant()
    {
        var tenantContext = _contextAccessor?.TenantContext;

        if (tenantContext == null || !tenantContext.HasResolvedTenant || tenantContext.Tenant == null)
            return NotFound("No tenant resolved");

        var tenant = tenantContext.Tenant;
        return Ok(new TenantInfo
        {
            Id = tenant.Id,
            Identifier = tenant.Identifier,
            Name = tenant.Name,
            IsActive = tenant.IsActive
        });
    }

    [HttpGet("check")]
    public ActionResult<bool> CheckTenantAvailable()
    {
        var isAvailable = _contextAccessor?.TenantContext?.HasResolvedTenant ?? false;
        return Ok(isAvailable);
    }
}

public class TenantInfo
{
    public KeyType Id { get; set; }
    public string? Identifier { get; set; }
    public string? Name { get; set; }
    public bool IsActive { get; set; }
}
