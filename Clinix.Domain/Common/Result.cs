namespace Clinix.Domain.Common;

/// <summary>Simple operation result (success/failure with optional error).</summary>
public class Result
    {
    public bool IsSuccess { get; }
    public string? Error { get; }
    public string? Message { get; }

    protected Result(bool isSuccess, string? error, string? message)
        {
        IsSuccess = isSuccess;
        Error = error;
        Message = message;
        }

    public static Result Success(string message) => new(true, null, message);
    public static Result Failure(string error) => new(false, error, null);
    }


/// <summary>
/// Simple operation result with optional data (for success/failure responses).
/// </summary>
public class Result<T> : Result
    {
    public T? Value { get; }

    private Result(bool isSuccess, T? value, string? error, string? message)
        : base(isSuccess, error, message)
        {
        Value = value;
        }

    public static Result<T> Success(T value, string message = "Operation succeeded")
        => new(true, value, null, message);

    public static new Result<T> Failure(string error)
        => new(false, default, error, null);
    }

