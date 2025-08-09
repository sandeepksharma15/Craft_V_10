namespace Craft.Core.Request;

public class GetPagedRequest<TKEY> : APIRequest<TKEY>
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

public class GetPagedRequest : GetPagedRequest<KeyType>
{
    public GetPagedRequest(int currentPage = 1, int pageSize = 10, bool includeDetails = false)
        : base(currentPage, pageSize, includeDetails) { }
}
