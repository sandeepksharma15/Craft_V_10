namespace Craft.Core.Request;

public class GetAllRequest : APIRequest
{
    public GetAllRequest(bool includeDetails = false) : base(ApiRequestType.GetAll)
    {
        IncludeDetails = includeDetails;
    }
}
