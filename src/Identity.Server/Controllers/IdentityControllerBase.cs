using System.Security.Claims;
using Identity.Core.Entities;
using Identity.Wrappers.Messages;
using Microsoft.AspNetCore.Mvc;
using Tools.TransactionsManager;

namespace Identity.Server.Controllers;

public abstract class IdentityControllerBase(
    ILogger<IdentityControllerBase> logger,
    ITransactionManager transaction,
    IHttpContextAccessor contextAccessor)
    : ControllerBase
{
    protected readonly ILogger<IdentityControllerBase> Logger = logger;
    protected readonly ITransactionManager Transaction = transaction;

    protected Guid UserId =>
        Guid.Parse(contextAccessor.HttpContext?.User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value
                   ?? throw new UnauthorizedAccessException("User not found"));

    protected Role Role =>
        (Role)Enum.Parse(typeof(Role),
            contextAccessor.HttpContext?.User.Claims.First(c => c.Type == ClaimTypes.Role).Value
            ?? throw new UnauthorizedAccessException("Role not found"));

    protected Guid ProviderId =>
        Guid.Parse(contextAccessor.HttpContext?.User.Claims.First(c => c.Type == ClaimTypes.Spn).Value
                   ?? throw new UnauthorizedAccessException("Provider not found"));

    protected void AppendCookie(string key, string value)
    {
        contextAccessor.HttpContext?.Response.Cookies.Append(key, value, new CookieOptions
        {
            HttpOnly = true,
            Expires = DateTime.UtcNow.AddDays(2)
        });
    }

    protected IActionResult Ok<T>(T data)
    {
        var response = new ApiResponse<T>()
        {
            Version = "1.0",
            Code = 200,
            Success = true,
            Message = "OK",
            Data = data
        };

        return base.Ok(response);
    }

    protected IActionResult Ok(string message)
    {
        var response = new ApiResponse
        {
            Version = "1.0",
            Code = 200,
            Success = true,
            Message = "OK"
        };

        return base.Ok(response);
    }

    protected IActionResult Error(Exception e)
    {
        var exceptionType = e.GetType().Name;
        int code;
        string message;
        switch (exceptionType)
        {
            case "DataException":
            case "ArgumentException":
                code = 400;
                message = e.Message;
                break;
            case "KeyNotFoundException":
            case "FileNotFoundException":
                code = 404;
                message = e.Message;
                break;
            default:
                code = 500;
                message = e.Message;
                break;
        }

        var response = new ApiResponse
        {
            Version = "1.0",
            Code = code,
            Success = false,
            Message = message
        };

        return base.StatusCode(response.Code, response);
    }
}