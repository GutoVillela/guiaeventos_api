using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Shared.Results;

public class PaginatedResult<TValue> : Result<PaginatedValue<TValue>> where TValue : class
{
    protected internal PaginatedResult(PaginatedValue<TValue>? value, bool isSuccess, ICollection<Error>? errors, FailureType? failureType) : base(value, isSuccess, null, errors, failureType)
    {
    }

    public static PaginatedResult<TValue> Success(IEnumerable<TValue> value, int currentPageIndex, int total, int pageSize)
    {
        PaginatedValue<TValue> result = new(currentPageIndex, total, pageSize, value);
        return new(result, true, null, null);
    }

    public static PaginatedResult<TValue> Success(PaginatedValue<TValue> result)
    {
        return new(result, true, null, null);
    }

    public static new PaginatedResult<TValue> Failure(ICollection<Error> errors, FailureType failureType)
    {
        return new(null, false, errors, failureType);
    }
}
