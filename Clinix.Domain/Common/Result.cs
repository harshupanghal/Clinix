namespace Clinix.Domain.Common;

/// <summary>Simple operation result (success/failure with optional error).</summary>
public class Result
    {
    public bool IsSuccess { get; }
    public string? Error { get; }
    public string? Message { get; }

    private Result(bool isSuccess, string? error, string? message)
        {
        IsSuccess = isSuccess;
        Error = error;
        Message = message;
        }

    public static Result Success(string message) => new(true, null, message);
    public static Result Failure(string error) => new(false, error, null);
    }

