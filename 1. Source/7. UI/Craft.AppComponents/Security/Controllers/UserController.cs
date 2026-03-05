using Craft.Controllers.ErrorHandling;
using Craft.Domain;
using Craft.QuerySpec.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Craft.AppComponents.Security;

[Route("api/[controller]")]
[ApiController]
public class UserController<TUser>(IUsersRepository<TUser, KeyType> repository,
    ILogger<EntityController<TUser, TUser, KeyType>> logger, IDatabaseErrorHandler databaseErrorHandler)
        : EntityController<TUser, TUser, KeyType>(repository, logger, databaseErrorHandler)
    where TUser : class, IEntity<KeyType>, IModel<KeyType>, new()
{
}
