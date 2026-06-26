namespace CarParking.Api.Models.DTO;

public class Result<T>
{
    public bool Success { get; }
    public T? Data { get; }
    public string? ErrorMessage { get; }

    public Result(bool success, T? data = default, string? errorMessage = null)
    {
        Success = success;
        Data = data;
        ErrorMessage = errorMessage;
    }

    public static Result<T> Failure(string errorMessage) => new Result<T>(false, default, errorMessage);
    public static Result<T> SuccessResult(T data) => new Result<T>(true, data);
}