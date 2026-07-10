namespace BotSaaS.Api.Shared.Results;

public class Result<T>
{
    // (Success = value) -- (Failure = message)
    public bool IsSuccess { get; }
    public T? Value { get; }
    public string? Error { get; }

    // Constructor
    private Result(bool isSuccess, T? value, string? error)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
    }

    // Methods to return 
    public static Result<T> Success(T value)
    {
        return new Result<T>(true, value, null); 
    }
    public static Result<T> Failure(string error)
    {
        return new Result<T>(false, default, error); 
    }
}