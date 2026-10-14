namespace Backend.Models.DTOs.Responses.Common;

public sealed class Result<T>
{
    public bool IsSuccess { get; private init; }

    public T? Value { get; private init; }

    public string? Error { get; private init; }

    public int StatusCode { get; private init; }

    public static Result<T> Ok(T value, int statusCode = StatusCodes.Status200OK)
    {
        return new Result<T>
        {
            IsSuccess = true,
            Value = value,
            StatusCode = statusCode
        };
    }

    public static Result<T> Fail(string error, int statusCode)
    {
        return new Result<T>
        {
            IsSuccess = false,
            Error = error,
            StatusCode = statusCode
        };
    }
}
