using Craft.Controllers.ErrorHandling;
using Craft.QuerySpec.Services;
using Craft.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Craft.AppComponents.Security;

[Route("api/[controller]")]
[ApiController]
public class RoleController(IRolesRepository<CraftRole, KeyType> repository,
    ILogger<EntityController<CraftRole, CraftRole, KeyType>> logger, IDatabaseErrorHandler databaseErrorHandler)
        : EntityController<CraftRole, CraftRole, KeyType>(repository, logger, databaseErrorHandler)
{
}
