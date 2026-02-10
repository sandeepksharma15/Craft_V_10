namespace Craft.Domain;

/// <summary>
/// Abstract base class for data transfer models with the default KeyType identifier.
/// Provides common properties for models including identity, concurrency control, and soft deletion.
/// </summary>
/// <remarks>
/// <para><b>Purpose:</b> BaseModel serves as a general-purpose data transfer model that can be used
/// for internal data transfer or scenarios where the specific input/output distinction of
/// <see cref="BaseDto"/> and <see cref="BaseVm"/> is not required.</para>
/// <para>
/// <b>When to use:</b>
/// <list type="bullet">
///   <item><description><see cref="BaseDto{TKey}"/> - Use for API <b>input</b> (requests from client)</description></item>
///   <item><description><see cref="BaseVm{TKey}"/> - Use for API <b>output</b> (responses to client)</description></item>
///   <item><description><see cref="BaseModel{TKey}"/> - Use for general data transfer where direction is not significant</description></item>
/// </list>
/// </para>
/// </remarks>
[Serializable]
public abstract class BaseModel : BaseModel<KeyType>, IDataTransferObject;

/// <summary>
/// Abstract base class for data transfer models with a strongly-typed identifier.
/// Provides common properties for models including identity, concurrency control, and soft deletion.
/// </summary>
/// <typeparam name="TKey">The type of the model identifier.</typeparam>
/// <remarks>
/// <para>This class implements <see cref="IDataTransferObject{TKey}"/> to indicate it is designed
/// for data transfer across application boundaries.</para>
/// <para>
/// <b>Properties included for data transfer:</b>
/// <list type="bullet">
///   <item><description><see cref="Id"/> - Entity identifier for references and updates</description></item>
///   <item><description><see cref="ConcurrencyStamp"/> - Optimistic concurrency for conflict detection</description></item>
///   <item><description><see cref="IsDeleted"/> - Soft-delete status for filtering and restoration</description></item>
/// </list>
/// </para>
/// </remarks>
[Serializable]
public abstract class BaseModel<TKey> : IDataTransferObject<TKey>
{
    /// <summary>
    /// Gets or sets the model identifier.
    /// </summary>
    public virtual TKey Id { get; set; } = default!;

    /// <summary>
    /// Gets or sets the concurrency stamp used for optimistic concurrency control.
    /// </summary>
    public virtual string? ConcurrencyStamp { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the model represents a soft-deleted entity.
    /// </summary>
    public virtual bool IsDeleted { get; set; }
}
