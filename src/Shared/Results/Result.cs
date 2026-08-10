namespace BotSaaS.Api.Shared.Results;

public enum ErrorType
{
    None = 0,
    Failure = 1,
    Conflict = 2,
    Unauthorized = 3
}
public class Result<T>
{
    // (Success = value) -- (Failure = message)
    public bool IsSuccess { get; }
    public T? Value { get; }
    public string? Error { get; }
    public ErrorType ErrorType { get; }

    // Constructor
    private Result(bool isSuccess, T? value, string? error, ErrorType errorType)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
        ErrorType = errorType;
    }

    // Methods to return 
    public static Result<T> Success(T value)
    {
        return new Result<T>(true, value, null, ErrorType.None); 
    }
    public static Result<T> Failure(string error)
    {
        return new Result<T>(false, default, error, ErrorType.Failure); 
    }
    public static Result<T> Conflict(string error)
    {
        return new Result<T>(false, default, error, ErrorType.Conflict);
    }
    public static Result<T> Unauthorized(string error)
    {
        return new Result<T>(false, default, error, ErrorType.Unauthorized);
    }
}