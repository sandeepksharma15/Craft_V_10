using System.Text.Json.Serialization;

namespace Craft.Core.Request;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(GetRequest), "get")]
[JsonDerivedType(typeof(GetAllRequest), "getAll")]
[JsonDerivedType(typeof(GetPagedRequest), "getPaged")]
public abstract class APIRequest<TKEY>
{
    public ApiRequestType RequestType { get; }
    public bool IncludeDetails { get; set; } = false;

    protected APIRequest(ApiRequestType requestType)
    {
        RequestType = requestType;
    }
}

public abstract class APIRequest : APIRequest<KeyType>
{
    protected APIRequest(ApiRequestType requestType) : base(requestType) { }
}
