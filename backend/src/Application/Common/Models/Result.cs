namespace backend.Application.Common.Models;

public class Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public string? Error { get; }
    public int? ErrorCode { get; }
    public string? Source { get; }

    private Result(bool isSuccess, T? value, string? error, int? errorCode, string? source)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
        ErrorCode = errorCode;
        Source = source;
    }

    public static Result<T> Success(T value)
    {
        return new Result<T>(true, value, null, null, null);
    }

    public static Result<T> Failure(string error, int? errorCode = null, string? source = null)
    {
        return new Result<T>(false, default, error, errorCode, source);
    }
}

// Non-generic Result for operations that don't return a value
public class Result
{
    public bool IsSuccess { get; }
    public string? Error { get; }
    public int? ErrorCode { get; }
    public string? Source { get; }

    private Result(bool isSuccess, string? error, int? errorCode, string? source)
    {
        IsSuccess = isSuccess;
        Error = error;
        ErrorCode = errorCode;
        Source = source;
    }

    public static Result Success()
    {
        return new Result(true, null, null, null);
    }

    public static Result Failure(string error, int? errorCode = null, string? source = null)
    {
        return new Result(false, error, errorCode, source);
    }
}