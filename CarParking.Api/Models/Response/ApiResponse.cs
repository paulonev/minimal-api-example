using System.Text.Json.Serialization;

namespace CarParking.Api.Models.Response;

public class ApiResponse<T>
{
    public T? Data { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ApiError? Error { get; set; }

    public bool IsSuccess => Error == null;

    public static ApiResponse<T> Success(T data) => new() { Data = data };

    public static ApiResponse<T> Failure(string title, string detail)
        => new() { Error = new ApiError(title, detail) };
}

public record ApiMessage(string Message, string? Code = null);
public record ApiError(string Title, string Detail, string? TraceId = null);
