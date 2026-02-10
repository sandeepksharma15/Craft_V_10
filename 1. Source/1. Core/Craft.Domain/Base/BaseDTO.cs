namespace Craft.Domain;

/// <summary>
/// Abstract base class for Data Transfer Objects (DTOs) with a strongly-typed identifier.
/// </summary>
/// <remarks>
/// <para><b>Purpose:</b> DTOs are used to transfer data between layers or across API boundaries,
/// particularly for <b>input operations</b> such as create and update requests.</para>
/// <para>
/// <b>When to use BaseDto vs BaseVm:</b>
/// <list type="bullet">
///   <item><description><see cref="BaseDto{TKey}"/> - Use for API <b>input</b> (requests from client to server)</description></item>
///   <item><description><see cref="BaseVm{TKey}"/> - Use for API <b>output</b> (responses from server to client)</description></item>
/// </list>
/// </para>
/// <para>
/// <b>Why ConcurrencyStamp and IsDeleted are included:</b>
/// <list type="bullet">
///   <item><description><see cref="IHasConcurrency.ConcurrencyStamp"/> - Required for optimistic concurrency during updates; 
///   client must send the stamp received from the last read to detect conflicts</description></item>
///   <item><description><see cref="ISoftDelete.IsDeleted"/> - Enables soft-delete/restore operations via API; 
///   allows client to request restoration of deleted records</description></item>
/// </list>
/// </para>
/// </remarks>
/// <typeparam name="TKey">The type of the DTO identifier.</typeparam>
/// <example>
/// <code>
/// // Input DTO for creating/updating a product
/// public class ProductDto : BaseDto
/// {
///     public string Name { get; set; } = string.Empty;
///     public decimal Price { get; set; }
///     public int CategoryId { get; set; }
/// }
/// 
/// // API Controller usage
/// [HttpPut("{id}")]
/// public async Task&lt;IActionResult&gt; Update(long id, ProductDto dto)
/// {
///     // ConcurrencyStamp from dto is used to detect conflicts
///     await _productService.UpdateAsync(id, dto);
///     return Ok();
/// }
/// </code>
/// </example>
[Serializable]
public abstract class BaseDto<TKey> : IDataTransferObject<TKey>
{
    /// <summary>
    /// Gets or sets the DTO identifier.
    /// </summary>
    public virtual TKey Id { get; set; } = default!;

    /// <summary>
    /// Gets or sets the concurrency stamp used for optimistic concurrency control.
    /// </summary>
    /// <remarks>
    /// When updating an entity via API, this stamp must match the current value in the database.
    /// A mismatch indicates the entity was modified by another user/process since it was last read.
    /// </remarks>
    public virtual string? ConcurrencyStamp { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the entity should be soft-deleted or restored.
    /// </summary>
    /// <remarks>
    /// <para>For create operations: typically false</para>
    /// <para>For update operations: set to true to soft-delete, false to restore</para>
    /// </remarks>
    public virtual bool IsDeleted { get; set; }
}

/// <summary>
/// Abstract base class for Data Transfer Objects (DTOs) with the default KeyType identifier.
/// </summary>
/// <inheritdoc cref="BaseDto{TKey}"/>
[Serializable]
public abstract class BaseDto : BaseDto<KeyType>, IDataTransferObject;
