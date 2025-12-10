namespace Craft.Domain;

/// <summary>
/// Abstract base class for simple Data Transfer Objects (DTOs) with a strongly-typed identifier.
/// This is a lightweight DTO without concurrency or soft-delete concerns.
/// </summary>
/// <typeparam name="TKey">The type of the DTO identifier.</typeparam>
[Serializable]
public abstract class BaseDto<TKey> : IModel<TKey>
{
    /// <summary>
    /// Gets or sets the DTO identifier.
    /// </summary>
    public virtual TKey Id { get; set; } = default!;
}

/// <summary>
/// Abstract base class for simple Data Transfer Objects (DTOs) with the default KeyType identifier.
/// This is a lightweight DTO without concurrency or soft-delete concerns.
/// </summary>
[Serializable]
public abstract class BaseDto : BaseDto<KeyType>, IModel;
