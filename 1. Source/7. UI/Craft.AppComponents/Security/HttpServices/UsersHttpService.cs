using Craft.Domain;
using Craft.QuerySpec;
using Microsoft.Extensions.Logging;

namespace Craft.AppComponents.Security;

public interface IUsersHttpService<T, ViewT, DataTransferT, TKey>: IHttpService<T, ViewT, DataTransferT, TKey>
        where T : class, IEntity<TKey>, IModel<TKey>, new()
        where ViewT : class, IModel<TKey>, new()
        where DataTransferT : class, IModel<TKey>, new()
{
}

public class UsersHttpService<T, ViewT, DataTransferT, TKey> : HttpService<T, ViewT, DataTransferT, TKey>, IUsersHttpService<T, ViewT, DataTransferT, TKey>
        where T : class, IEntity<TKey>, IModel<TKey>, new()
        where ViewT : class, IModel<TKey>, new()
        where DataTransferT : class, IModel<TKey>, new()
{
    public UsersHttpService(Uri apiURL, HttpClient httpClient, ILogger<HttpService<T, ViewT, DataTransferT, TKey>> logger) 
        : base(apiURL, httpClient, logger) { }
}
