using System.Text.Json.Serialization;

namespace Craft.Core.Request;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(GetRequest), "get")]
[JsonDerivedType(typeof(GetAllRequest), "getAll")]
[JsonDerivedType(typeof(GetPagedRequest), "getPaged")]
/// <summary>
/// Represents a base request for retrieving entities by key.
/// </summary>
/// <typeparam name="TKEY">The type of the entity key.</typeparam>
public abstract class ApiGetRequest<TKEY>
{
    /// <summary>
    /// Gets the type of the API request.
    /// </summary>
    public ApiRequestType RequestType { get; }

    /// <summary>
    /// Gets or sets a value indicating whether to include details in the response.
    /// </summary>
    public bool IncludeDetails { get; set; } = false;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiGetRequest{TKEY}"/> class.
    /// </summary>
    /// <param name="requestType">The type of the API request.</param>
    protected ApiGetRequest(ApiRequestType requestType)
    {
        RequestType = requestType;
    }
}

/// <summary>
/// Represents a base request for retrieving entities by key using the default key type.
/// </summary>
public abstract class ApiGetRequest : ApiGetRequest<KeyType>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ApiGetRequest"/> class.
    /// </summary>
    /// <param name="requestType">The type of the API request.</param>
    protected ApiGetRequest(ApiRequestType requestType) : base(requestType) { }
}
