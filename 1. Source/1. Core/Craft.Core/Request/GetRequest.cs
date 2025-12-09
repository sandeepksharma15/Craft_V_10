namespace Craft.Core.Request;

public class GetRequest<TKEY> : APIRequest
{
    public TKEY Id { get; set; }

    public GetRequest(TKEY id = default!, bool includeDetails = false) : base(ApiRequestType.Get)
    {
        Id = id;
        IncludeDetails = includeDetails;
    }
}

public class GetRequest : GetRequest<KeyType>
{
    public GetRequest(KeyType id, bool includeDetails) : base(id, includeDetails) { }

    public GetRequest() : base(default!, false) { }
}
