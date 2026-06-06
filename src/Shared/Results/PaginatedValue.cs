namespace Shared.Results;

public class PaginatedValue<TValue> where TValue : class
{
    public int CurrentPageIndex { get; init; }
    public int Total { get; init; }
    public int PageSize { get; init; }
    public IEnumerable<TValue> Result { get; init; }

    public bool HasResults => Result.Any();

    public PaginatedValue(int currentPageIndex, int total, int pageSize, IEnumerable<TValue> result)
    {
        CurrentPageIndex = currentPageIndex;
        Total = total;
        PageSize = pageSize;
        Result = result;
    }
}
