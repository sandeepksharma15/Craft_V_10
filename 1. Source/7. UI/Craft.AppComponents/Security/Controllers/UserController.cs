using Craft.Controllers.ErrorHandling;
using Craft.QuerySpec.Services;
using Craft.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Craft.AppComponents.Security;

[Route("api/[controller]")]
[ApiController]
public class UserController(IUsersRepository<CraftUser, KeyType> repository,
    ILogger<EntityController<CraftUser, CraftUser, KeyType>> logger, IDatabaseErrorHandler databaseErrorHandler)
        : EntityController<CraftUser, CraftUser, KeyType>(repository, logger, databaseErrorHandler)
{
}
