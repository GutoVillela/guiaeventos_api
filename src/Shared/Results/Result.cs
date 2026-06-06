using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Shared.Results;

public class Result
{
    public enum FailureType
    {
        BadRequest,
        NotFound,
        Unauthorized,
        Forbidden,
        Conflict,
        InternalServerError
    }

    public bool IsSuccess { get; init; }
    public bool IsFailure => !IsSuccess;
    public string? Message { get; init; }
    public ICollection<Error>? Errors { get; init; } = [];
    public FailureType? ErrorType { get; init; }

    protected internal Result(bool isSuccess, string? message, ICollection<Error>? errors, FailureType? errorType)
    {
        if (isSuccess && errors?.Count > 0)
        {
            throw new SuccessResultWithErrorsException();
        }

        if (!isSuccess && errors?.Count == 0)
        {
            throw new FailureResultWithoutErrorsException();
        }

        if (!isSuccess && errorType == null)
        {
            throw new FailureResultWithoutFailureTypeException();
        }

        IsSuccess = isSuccess;
        Message = message;
        Errors = errors;
        ErrorType = errorType;
    }

    public static Result Success(string message) => new(true, message, null, null);
    public static Result Success() => new(true, null, null, null);
    public static Result Failure(ICollection<Error> errors, FailureType failureType) => new(false, null, errors, failureType);
    public static Result Failure(string message, ICollection<Error> errors, FailureType failureType) => new(false, message, errors, failureType);

    public static Result<TValue> Success<TValue>(TValue value, string message) where TValue : class => new(value, true, message, null, null);
    public static Result<TValue> Success<TValue>(TValue value) where TValue : class => new(value, true, null, null, null);
    public static Result<TValue> Failure<TValue>(string message, ICollection<Error> errors, FailureType failureType) where TValue : class => new(null, false, message, errors, failureType);
    public static Result<TValue> Failure<TValue>(ICollection<Error> errors, FailureType failureType) where TValue : class => new(null, false, null, errors, failureType);


}

public class Result<TValue> : Result where TValue : class
{
    private readonly TValue? _value;
    public TValue? Value => IsSuccess ? _value : throw new InvalidOperationException("Cannot access value for a failed result");

    protected internal Result(TValue? value, bool isSuccess, string? message, ICollection<Error>? errors, FailureType? failureType) : base(isSuccess, message, errors, failureType)
    {
        _value = value;
    }

    public static implicit operator Result<TValue>(TValue value) => Result.Success(value);
}

public class SuccessResultWithErrorsException() : ArgumentException("A success result can't have errors");

public class FailureResultWithoutErrorsException() : InvalidOperationException("A failure result must have errors.");

public class FailureResultWithoutFailureTypeException() : ArgumentException("A failure result must have a failure type.");