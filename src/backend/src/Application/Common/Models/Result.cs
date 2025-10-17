namespace backend.Application.Common.Models;

public class Result<T>
{
    public bool IsSuccess { get; private set; }
    public string? Error { get; private set; }
    public string? ErrorSource { get; private set; } // e.g., Validation, Database, Exception, ExternalService
    public T? Value { get; private set; }

    public static Result<T> Success(T value) => new() { IsSuccess = true, Value = value };
    public static Result<T> Failure(string error, string errorSource = "Unknown") =>
        new()
        { IsSuccess = false, Error = error, ErrorSource = errorSource };
}

// Non-generic Result for operations that don't return a value
public class Result
{
    public bool IsSuccess { get; private set; }
    public string? Error { get; private set; }
    public string? ErrorSource { get; private set; }

    public static Result Success() => new() { IsSuccess = true };
    public static Result Failure(string error, string errorSource = "Unknown") =>
        new()
        { IsSuccess = false, Error = error, ErrorSource = errorSource };
}
