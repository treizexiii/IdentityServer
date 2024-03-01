namespace Identity.Services.Factories;

public class ServiceResult
{
    public bool Success { get; set; }
    public int StatusCode { get; set; } = 200;
    public string[]? Errors { get; set; }
}

public class ServiceResult<T> : ServiceResult
{
    public T? Data { get; set; }
}

internal static class ServiceResultFactory
{
    public static ServiceResult Ok()
    {
        return new ServiceResult { Success = true };
    }

    public static ServiceResult Fail(string message, int code)
    {
        return new ServiceResult
        {
            Success = false, Errors = [message]
        };
    }

    public static ServiceResult Fail(string[] errors, int code)
    {
        return new ServiceResult { Success = false, Errors = errors };
    }
}

internal static class ServiceResultFactory<T>
{
    public static ServiceResult<T> Ok(T data)
    {
        return new ServiceResult<T> { Success = true, Data = data };
    }

    public static ServiceResult<T> Fail(string message, int code)
    {
        return new ServiceResult<T> { Success = false, Data = default, Errors = [message] };
    }

    public static ServiceResult<T> Fail(string[] errors, int code)
    {
        return new ServiceResult<T> { Success = false, Data = default, Errors = errors };
    }

    public static ServiceResult<T> Fail(string message, int code, T? data)
    {
        return new ServiceResult<T> { Success = false, Data = data, Errors = [message] };
    }

    public static ServiceResult<T> Fail(string[] errors, int code, T? data)
    {
        return new ServiceResult<T> { Success = false, Data = data, Errors = errors };
    }

    public static ServiceResult<T> NotFound()
    {
        return new ServiceResult<T>
            { Success = false, StatusCode = 404, Data = default, Errors = ["Not found"] };
    }

    public static ServiceResult<T> Forbid()
    {
        return new ServiceResult<T>
            { Success = false, StatusCode = 403, Data = default, Errors = ["Forbidden"] };
    }
}