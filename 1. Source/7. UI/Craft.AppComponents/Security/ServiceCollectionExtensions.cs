using Craft.Domain;
using Craft.QuerySpec.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Craft.AppComponents.Security;

/// <summary>
/// Extension methods for registering security services in the dependency injection container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers security repositories and controllers for the API layer.
    /// Adds typed <see cref="IUsersRepository{T,TKey}"/> and <see cref="IRolesRepository{T,TKey}"/>
    /// repositories, and uses a <see cref="SecurityControllerFeatureProvider{TUser,TRole}"/> to
    /// dynamically register the closed-generic <see cref="UserController{TUser}"/> and
    /// <see cref="RoleController{TRole}"/> without requiring concrete controller classes in the host app.
    /// </summary>
    /// <typeparam name="TUser">The application user entity type.</typeparam>
    /// <typeparam name="TRole">The application role entity type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddSecurityApi<TUser, TRole>(this IServiceCollection services)
        where TUser : class, IEntity<KeyType>, IModel<KeyType>, new()
        where TRole : class, IEntity<KeyType>, IModel<KeyType>, new()
    {
        services.AddScoped<IUsersRepository<TUser, KeyType>, UsersRepository<TUser, KeyType>>();
        services.AddScoped<IRolesRepository<TRole, KeyType>, RolesRepository<TRole, KeyType>>();

        services.AddControllers()
            .ConfigureApplicationPartManager(apm =>
                apm.FeatureProviders.Add(new SecurityControllerFeatureProvider<TUser, TRole>()));

        return services;
    }

    /// <summary>
    /// Registers security HTTP services for the Blazor UI layer.
    /// For each of users and roles, three interfaces are registered:
    /// <list type="bullet">
    ///   <item><description>The strongly-typed <see cref="IUsersHttpService{T,ViewT,DataTransferT,TKey}"/> /
    ///   <see cref="IRolesHttpService{T,ViewT,DataTransferT,TKey}"/> — for edit components.</description></item>
    ///   <item><description><see cref="IHttpService{T,ViewT,DataTransferT,TKey}"/> — for generic keyed access.</description></item>
    ///   <item><description><see cref="IHttpService{T}"/> (simplified) — for list components.</description></item>
    /// </list>
    /// </summary>
    /// <typeparam name="TUser">The application user entity type.</typeparam>
    /// <typeparam name="TUserVM">The user view model type.</typeparam>
    /// <typeparam name="TUserDTO">The user data transfer object type.</typeparam>
    /// <typeparam name="TRole">The application role entity type.</typeparam>
    /// <typeparam name="TRoleVM">The role view model type.</typeparam>
    /// <typeparam name="TRoleDTO">The role data transfer object type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="httpClientFactory">Factory function to resolve the <see cref="HttpClient"/> from DI.</param>
    /// <param name="baseAddress">The base address of the API (e.g., "https+http://api").</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddSecurityUI<TUser, TUserVM, TUserDTO, TRole, TRoleVM, TRoleDTO>(
        this IServiceCollection services,
        Func<IServiceProvider, HttpClient> httpClientFactory,
        string baseAddress)
        where TUser : class, IEntity, IModel, new()
        where TUserVM : class, IModel, new()
        where TUserDTO : class, IModel, new()
        where TRole : class, IEntity, IModel, new()
        where TRoleVM : class, IModel, new()
        where TRoleDTO : class, IModel, new()
    {
        // Users: registers UsersHttpService, IHttpService<TUser,TUserVM,TUserDTO,KeyType>, and IHttpService<TUser>
        services.AddTransientCustomHttpService<TUser, TUserVM, TUserDTO, UsersHttpService<TUser, TUserVM, TUserDTO, KeyType>>(
            httpClientFactory, baseAddress, "/api/User",
            registerPrimaryInterface: false, registerWithKeyType: true, registerSimplified: true);

        // Forward the strongly-typed users interface to the already-registered service
        services.AddTransient<IUsersHttpService<TUser, TUserVM, TUserDTO, KeyType>>(sp =>
            sp.GetRequiredService<UsersHttpService<TUser, TUserVM, TUserDTO, KeyType>>());

        // Roles: registers RolesHttpService, IHttpService<TRole,TRoleVM,TRoleDTO,KeyType>, and IHttpService<TRole>
        services.AddTransientCustomHttpService<TRole, TRoleVM, TRoleDTO, RolesHttpService<TRole, TRoleVM, TRoleDTO, KeyType>>(
            httpClientFactory, baseAddress, "/api/Role",
            registerPrimaryInterface: false, registerWithKeyType: true, registerSimplified: true);

        // Forward the strongly-typed roles interface to the already-registered service
        services.AddTransient<IRolesHttpService<TRole, TRoleVM, TRoleDTO, KeyType>>(sp =>
            sp.GetRequiredService<RolesHttpService<TRole, TRoleVM, TRoleDTO, KeyType>>());

        return services;
    }
}

