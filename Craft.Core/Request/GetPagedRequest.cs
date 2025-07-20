namespace Craft.Core.Request;

public class GetPagedRequest : APIRequest
{
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }

    public GetPagedRequest(int currentPage = 1, int pageSize = 10, bool includeDetails = false) : base(ApiRequestType.GetPaged)
    {
        CurrentPage = currentPage;
        PageSize = pageSize;
        IncludeDetails = includeDetails;
    }
}
