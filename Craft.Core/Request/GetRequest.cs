namespace Craft.Core.Request;

public class GetRequest : APIRequest
{
    public int Id { get; set; }

    public GetRequest(int id, bool includeDetails) : base(ApiRequestType.Get)
    {
        Id = id;
        IncludeDetails = includeDetails;
    }
}
