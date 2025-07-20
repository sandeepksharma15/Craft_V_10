namespace Craft.Core.Request;

public class GetPagedRequest : APIRequest
{
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }

    public GetPagedRequest(int currentPage, int pageSize, bool includeDetails) : base(ApiRequestType.GetPaged)
    {
        CurrentPage = currentPage;
        PageSize = pageSize;
        IncludeDetails = includeDetails;
    }
}
