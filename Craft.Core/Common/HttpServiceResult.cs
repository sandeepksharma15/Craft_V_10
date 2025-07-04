namespace Craft.Core.Common;

public class HttpServiceResult<T>
{
    public T? Data { get; set; }
    public bool Success { get; set; }
    public List<string>? Errors { get; set; }
    public int? StatusCode { get; set; }
}
