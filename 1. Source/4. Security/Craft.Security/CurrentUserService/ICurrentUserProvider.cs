using System.Security.Claims;

namespace Craft.Security;

public interface ICurrentUserProvider
{
    ClaimsPrincipal? GetUser();
}
