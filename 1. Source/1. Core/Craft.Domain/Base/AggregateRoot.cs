namespace Craft.Domain;

/// <summary> 
/// Base type for aggregate roots. 
/// </summary> 
public abstract class AggregateRoot<TKey> : BaseEntity<TKey> 
{ 
    protected AggregateRoot() { } 

    protected AggregateRoot(TKey id) : base(id) { } 
}
