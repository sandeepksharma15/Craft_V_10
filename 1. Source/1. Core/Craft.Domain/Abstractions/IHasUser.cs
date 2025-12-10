namespace Craft.Domain;

/// <summary>
/// Defines a contract for entities that are associated with a specific user.
/// </summary>
/// <typeparam name="TKey">The type of the user identifier.</typeparam>
public interface IHasUser<TKey>
{
    /// <summary>
    /// The name of the database column for the UserId property.
    /// </summary>
    public const string ColumnName = "UserId";

    /// <summary>
    /// Gets or sets the user identifier.
    /// </summary>
    TKey UserId { get; set; }

    /// <summary>
    /// Gets the user identifier.
    /// </summary>
    /// <returns>The user identifier.</returns>
    TKey GetUserId() => UserId;

    /// <summary>
    /// Determines whether the user identifier is set to a non-default value.
    /// </summary>
    /// <returns>True if the user identifier is set; otherwise, false.</returns>
    bool IsUserIdSet() => !(UserId!.Equals(default(TKey)));

    /// <summary>
    /// Sets the user identifier.
    /// </summary>
    /// <param name="userId">The user identifier to set.</param>
    void SetUserId(TKey userId) => UserId = userId;
}

/// <summary>
/// Defines a contract for entities that are associated with a user with the default KeyType identifier.
/// </summary>
public interface IHasUser : IHasUser<KeyType>;
