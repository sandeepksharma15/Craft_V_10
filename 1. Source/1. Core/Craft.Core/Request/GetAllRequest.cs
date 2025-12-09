namespace Craft.Core.Request;

public class GetAllRequest<TKEY> : APIRequest<TKEY>
{
    public GetAllRequest(bool includeDetails = false) : base(ApiRequestType.GetAll)
    {
        IncludeDetails = includeDetails;
    }
}

public class GetAllRequest : GetAllRequest<KeyType>
{
    public GetAllRequest(bool includeDetails = false) : base(includeDetails) { }
}
