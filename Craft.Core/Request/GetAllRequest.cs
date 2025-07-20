namespace Craft.Core.Request;

public class GetAllRequest : APIRequest
{
    public GetAllRequest(bool includeDetails) : base(ApiRequestType.GetAll)
    {
        IncludeDetails = includeDetails;
    }
}
