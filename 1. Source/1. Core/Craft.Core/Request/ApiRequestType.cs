namespace Craft.Core.Request;

/// <summary>
/// Specifies the type of API request.
/// </summary>
public enum ApiRequestType
{
    /// <summary>
    /// Request to get a single entity by key.
    /// </summary>
    Get,

    /// <summary>
    /// Request to get all entities.
    /// </summary>
    GetAll,

    /// <summary>
    /// Request to get entities in a paged format.
    /// </summary>
    GetPaged
}
