namespace Craft.Domain;

/// <summary>
/// Defines the contract for data transfer objects used in API communication.
/// </summary>
/// <remarks>
/// <para>This interface is implemented by both <see cref="BaseDto{TKey}"/> and <see cref="BaseVm{TKey}"/>
/// to ensure consistent API contract behavior.</para>
/// <para>
/// <b>Properties required for API communication:</b>
/// <list type="bullet">
///   <item><description><see cref="IHasId{TKey}.Id"/> - Entity identifier for updates and references</description></item>
///   <item><description><see cref="IHasConcurrency.ConcurrencyStamp"/> - Optimistic concurrency for conflict detection</description></item>
///   <item><description><see cref="ISoftDelete.IsDeleted"/> - Soft-delete status for filtering and restoration</description></item>
/// </list>
/// </para>
/// </remarks>
/// <typeparam name="TKey">The type of the identifier.</typeparam>
public interface IDataTransferObject<TKey> : IModel<TKey>, IHasConcurrency, ISoftDelete;

/// <summary>
/// Defines the contract for data transfer objects with the default KeyType identifier.
/// </summary>
public interface IDataTransferObject : IDataTransferObject<KeyType>, IModel;
