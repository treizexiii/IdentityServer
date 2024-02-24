namespace Identity.Wrappers.Messages;

public class ApiResponse
{
    public string Version { get; set; } = string.Empty;
    public int Code { get; set; }
    public bool Success { get; set; }
    public string Message { get; set; } = null!;
    public string[]? Errors { get; set; }
}

public class ApiResponse<T> : ApiResponse
{
    public T? Data { get; set; }
}

public static class ApiResponseExtension
{
    public static ApiResponse Success()
    {
        return new ApiResponse
        {
            Version = "1.0",
            Code = 200,
            Success = true,
            Message = "OK"
        };
    }
}