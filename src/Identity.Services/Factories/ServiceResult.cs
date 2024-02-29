namespace Identity.Services.Factories;

public class ServiceResult
{
    public bool Success { get; set; }
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

    public static ServiceResult Fail(string message)
    {
        return new ServiceResult
        {
            Success = false, Errors = [message]
        };
    }

    public static ServiceResult Fail(string[] errors)
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

    public static ServiceResult<T> Fail(string message)
    {
        return new ServiceResult<T> { Success = false, Data = default, Errors = [message] };
    }

    public static ServiceResult<T> Fail(string[] errors)
    {
        return new ServiceResult<T> { Success = false, Data = default, Errors = errors };
    }

    public static ServiceResult<T> Fail(string message, T? data)
    {
        return new ServiceResult<T> { Success = false, Data = data,Errors = [message] };
    }

    public static ServiceResult<T> Fail(string[] errors, T? data)
    {
        return new ServiceResult<T> { Success = false, Data = data, Errors = errors };
    }
}