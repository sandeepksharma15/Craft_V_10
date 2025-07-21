using System.Text.Json.Serialization;

namespace Craft.Core.Request;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(GetRequest), "get")]
[JsonDerivedType(typeof(GetAllRequest), "getAll")]
[JsonDerivedType(typeof(GetPagedRequest), "getPaged")]
public abstract class ApiGetRequest<TKEY>
{
    public ApiRequestType RequestType { get; }
    public bool IncludeDetails { get; set; } = false;

    protected ApiGetRequest(ApiRequestType requestType)
    {
        RequestType = requestType;
    }
}

public abstract class ApiGetRequest : ApiGetRequest<KeyType>
{
    protected ApiGetRequest(ApiRequestType requestType) : base(requestType) { }
}
