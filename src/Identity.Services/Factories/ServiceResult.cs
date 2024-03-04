using Identity.Wrappers.Dto;

namespace Identity.Services.Factories;

public class ServiceResult
{
    public bool Success { get; set; }
    public int StatusCode { get; set; } = 200;
    public string[]? Errors { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class ServiceResult<T> : ServiceResult
{
    public T? Data { get; set; }
}

internal static class ServiceResultFactory
{
    public static ServiceResult Ok()
    {
        return new ServiceResult { Success = true, StatusCode = 200 };
    }

    public static ServiceResult Ok(string message)
    {
        return new ServiceResult { Success = true, StatusCode = 200, Message = message };
    }

    public static ServiceResult Fail(string message, int code)
    {
        return new ServiceResult
        {
            Success = false, StatusCode = code, Errors = [message]
        };
    }

    public static ServiceResult Fail(string[] errors, int code)
    {
        return new ServiceResult { Success = false, StatusCode = code, Errors = errors };
    }

    public static ServiceResult NotFound()
    {
        return new ServiceResult
            { Success = false, StatusCode = 404, Message = "Not found", Errors = ["Not found"] };
    }

    public static ServiceResult Forbid()
    {
        return new ServiceResult
            { Success = false, StatusCode = 403, Message = "Forbidden", Errors = ["Forbidden"] };
    }

    public static ServiceResult BadRequest(string message)
    {
        return new ServiceResult { Success = false, StatusCode = 400, Message = message, Errors = [message] };
    }

    public static ServiceResult Unauthorized(string message)
    {
        return new ServiceResult { Success = false, StatusCode = 401, Message = message, Errors = [message] };
    }
}

internal static class ServiceResultFactory<T>
{
    public static ServiceResult<T> Ok(T data)
    {
        return new ServiceResult<T> { Success = true, StatusCode = 200, Data = data };
    }

    public static ServiceResult<T> Ok(string message, T data)
    {
        return new ServiceResult<T> { Success = true, StatusCode = 200, Message = message, Data = data };
    }

    public static ServiceResult<T> Fail(string message, int code)
    {
        return new ServiceResult<T> { Success = false, StatusCode = code, Data = default, Errors = [message] };
    }

    public static ServiceResult<T> Fail(string[] errors, int code)
    {
        return new ServiceResult<T> { Success = false, StatusCode = code, Data = default, Errors = errors };
    }

    public static ServiceResult<T> Fail(string message, int code, T? data)
    {
        return new ServiceResult<T> { Success = false, StatusCode = code, Data = data, Errors = [message] };
    }

    public static ServiceResult<T> Fail(string[] errors, int code, T? data)
    {
        return new ServiceResult<T> { Success = false, StatusCode = code, Data = data, Errors = errors };
    }

    public static ServiceResult<T> NotFound()
    {
        return new ServiceResult<T>
            { Success = false, StatusCode = 404, Message = "Not found", Data = default, Errors = ["Not found"] };
    }

    public static ServiceResult<T> Forbid()
    {
        return new ServiceResult<T>
            { Success = false, StatusCode = 403, Message = "Forbidden", Data = default, Errors = ["Forbidden"] };
    }

    public static ServiceResult<T> BadRequest(string message, T? data)
    {
        return new ServiceResult<T>
            { Success = false, StatusCode = 400, Message = message, Data = data, Errors = [message] };
    }

    public static ServiceResult<T> Unauthorized(string message, T? data)
    {
        return new ServiceResult<T>
            { Success = false, StatusCode = 401, Message = message, Data = data, Errors = [message] };
    }

    public static ServiceResult<AppDto> Conflict(string message)
    {
        return new ServiceResult<AppDto>
        {
            Success = false,
            StatusCode = 409,
            Message = message,
            Data = default,
            Errors = [message]
        };
    }
}