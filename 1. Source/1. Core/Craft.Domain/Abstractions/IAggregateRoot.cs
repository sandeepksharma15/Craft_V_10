namespace Craft.Domain;

/// <summary>
/// Marker interface for aggregate roots with a strongly-typed identifier.
/// </summary>
/// <typeparam name="TKey">The type of the aggregate root identifier.</typeparam>
/// <remarks>
/// <para>An aggregate root is the entry point to an aggregate - a cluster of domain objects
/// that can be treated as a single unit for data changes.</para>
/// <para>
/// <b>Aggregate root rules:</b>
/// <list type="bullet">
///   <item><description>External objects can only reference the aggregate root, not internal entities</description></item>
///   <item><description>All changes to the aggregate must go through the root</description></item>
///   <item><description>Repositories should only work with aggregate roots</description></item>
///   <item><description>Aggregate roots are the consistency boundary for transactions</description></item>
///   <item><description>Domain events are typically raised only from aggregate roots</description></item>
/// </list>
/// </para>
/// <para>
/// <b>Usage example:</b>
/// <code>
/// public class Order : BaseEntity, IAggregateRoot, IHasDomainEvents
/// {
///     private readonly List&lt;OrderLine&gt; _orderLines = [];
///     private readonly List&lt;IDomainEvent&gt; _domainEvents = [];
///     
///     public IReadOnlyCollection&lt;OrderLine&gt; OrderLines => _orderLines.AsReadOnly();
///     public IReadOnlyCollection&lt;IDomainEvent&gt; DomainEvents => _domainEvents.AsReadOnly();
///     
///     public void AddOrderLine(Product product, int quantity)
///     {
///         var line = new OrderLine(product, quantity);
///         _orderLines.Add(line);
///         AddDomainEvent(new OrderLineAddedEvent(Id, line.Id));
///     }
///     
///     // IHasDomainEvents implementation...
/// }
/// </code>
/// </para>
/// </remarks>
public interface IAggregateRoot<TKey> : IEntity<TKey>;

/// <summary>
/// Marker interface for aggregate roots with the default KeyType identifier.
/// </summary>
public interface IAggregateRoot : IAggregateRoot<KeyType>, IEntity;
