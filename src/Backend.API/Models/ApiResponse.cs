namespace Backend.API.Models;

public class ApiResponse
{
    public bool Success { get; init; }
    public string? Message { get; init; }
    public IDictionary<string, string[]>? Errors { get; init; }
    public string? TraceId { get; init; }

    public static ApiResponse Fail(string message, string? traceId = null, IDictionary<string, string[]>? errors = null) =>
        new() { Success = false, Message = message, TraceId = traceId, Errors = errors };
}

public class ApiResponse<T> : ApiResponse
{
    public T? Data { get; init; }

    public static ApiResponse<T> Ok(T data, string? message = null) =>
        new() { Success = true, Data = data, Message = message };
}