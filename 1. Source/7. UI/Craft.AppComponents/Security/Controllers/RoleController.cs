using Craft.Controllers.ErrorHandling;
using Craft.Domain;
using Craft.QuerySpec.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Craft.AppComponents.Security;

[Route("api/[controller]")]
[ApiController]
public class RoleController<TRole>(IRolesRepository<TRole, KeyType> repository, ILogger<EntityController<TRole, TRole, KeyType>> logger, 
    IDatabaseErrorHandler databaseErrorHandler) : EntityController<TRole, TRole, KeyType>(repository, logger, databaseErrorHandler)
        where TRole : class, IEntity<KeyType>, IModel<KeyType>, new()
{
}
