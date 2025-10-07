namespace Clinix.Domain.Common;

/// <summary>Simple operation result (success/failure with optional error).</summary>
public class Result
    {
    public bool IsSuccess { get; }
    public string? Error { get; }

    private Result(bool isSuccess, string? error)
        {
        IsSuccess = isSuccess;
        Error = error;
        }

    public static Result Success() => new(true, null);
    public static Result Failure(string error) => new(false, error);
    }

