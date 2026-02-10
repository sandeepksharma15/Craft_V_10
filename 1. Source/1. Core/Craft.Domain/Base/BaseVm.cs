namespace Craft.Domain;

/// <summary>
/// Abstract base class for View Models (VMs) with the default KeyType identifier.
/// </summary>
/// <inheritdoc cref="BaseVm{TKey}"/>
[Serializable]
public abstract class BaseVm : BaseVm<KeyType>, IDataTransferObject;

/// <summary>
/// Abstract base class for View Models (VMs) with a strongly-typed identifier.
/// </summary>
/// <remarks>
/// <para><b>Purpose:</b> View Models are used to transfer data from server to client,
/// particularly for <b>output operations</b> such as API responses and display data.</para>
/// <para>
/// <b>When to use BaseVm vs BaseDto:</b>
/// <list type="bullet">
///   <item><description><see cref="BaseVm{TKey}"/> - Use for API <b>output</b> (responses from server to client)</description></item>
///   <item><description><see cref="BaseDto{TKey}"/> - Use for API <b>input</b> (requests from client to server)</description></item>
/// </list>
/// </para>
/// <para>
/// <b>Why ConcurrencyStamp and IsDeleted are included:</b>
/// <list type="bullet">
///   <item><description><see cref="IHasConcurrency.ConcurrencyStamp"/> - Client receives this stamp with the response; 
///   must be sent back on subsequent updates to enable optimistic concurrency conflict detection</description></item>
///   <item><description><see cref="ISoftDelete.IsDeleted"/> - Indicates the entity's deletion status to the client; 
///   allows UI to display appropriate status or enable restore functionality</description></item>
/// </list>
/// </para>
/// <para>
/// <b>View Model vs DTO pattern:</b> While both carry similar properties, their semantic intent differs:
/// <list type="bullet">
///   <item><description>VM: "Here's what the entity looks like" (read-optimized, may include computed/display properties)</description></item>
///   <item><description>DTO: "Here's what I want to change" (write-optimized, contains only editable properties)</description></item>
/// </list>
/// </para>
/// </remarks>
/// <typeparam name="TKey">The type of the view model identifier.</typeparam>
/// <example>
/// <code>
/// // Output VM for displaying a product
/// public class ProductVm : BaseVm
/// {
///     public string Name { get; set; } = string.Empty;
///     public decimal Price { get; set; }
///     public string CategoryName { get; set; } = string.Empty; // Denormalized for display
///     public string FormattedPrice => Price.ToString("C");      // Computed property
/// }
/// 
/// // API Controller usage
/// [HttpGet("{id}")]
/// public async Task&lt;ActionResult&lt;ProductVm&gt;&gt; Get(long id)
/// {
///     var vm = await _productService.GetByIdAsync&lt;ProductVm&gt;(id);
///     // ConcurrencyStamp is included so client can send it back on updates
///     return Ok(vm);
/// }
/// </code>
/// </example>
[Serializable]
public abstract class BaseVm<TKey> : IDataTransferObject<TKey>
{
    /// <summary>
    /// Gets or sets the view model identifier.
    /// </summary>
    public virtual TKey Id { get; set; } = default!;

    /// <summary>
    /// Gets or sets the concurrency stamp for optimistic concurrency control.
    /// </summary>
    /// <remarks>
    /// This value should be stored by the client and sent back on subsequent update requests
    /// to enable the server to detect concurrent modifications.
    /// </remarks>
    public virtual string? ConcurrencyStamp { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the entity is soft-deleted.
    /// </summary>
    /// <remarks>
    /// When true, the UI can display appropriate visual indicators or enable restore functionality.
    /// </remarks>
    public virtual bool IsDeleted { get; set; }
}
