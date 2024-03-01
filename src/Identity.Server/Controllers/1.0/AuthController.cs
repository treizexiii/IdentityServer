using Asp.Versioning;
using Identity.Core.Entities;
using Identity.Services.Auth;
using Identity.Wrappers.Dto;
using Microsoft.AspNetCore.Mvc;
using Tools.TransactionsManager;

namespace Identity.Server.Controllers._1._0;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Produces("application/json")]
public class AuthController(
    ILogger<AuthController> logger,
    ITransactionManager transaction,
    IHttpContextAccessor contextAccessor,
    IAuthService authService)
    : IdentityControllerBase(logger, transaction, contextAccessor)
{
    [HttpPost]
    [Route("register")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> RegisterAsync(RegisterDto registerDto)
    {
        var guid = Guid.NewGuid();
        try
        {
            Logger.LogInformation("Register request");
            await Transaction.BeginTransactionAsync(guid);

            var result = await authService.RegisterAsync(registerDto, RolesList.User);
            if (!result.Success)
            {
                await Transaction.RollbackTransactionAsync(guid, "Bad request");
                return BadRequest(result);
            }

            await Transaction.CommitTransactionAsync(guid);
            return Ok("User registered");
        }
        catch (Exception e)
        {
            await Transaction.RollbackTransactionAsync(guid, e.Message);
            return Error(e);
        }
    }

    [HttpPost]
    [Route("login")]
    [ProducesResponseType(typeof(JwtToken), StatusCodes.Status200OK)]
    public async Task<IActionResult> LoginAsync(LoginDto loginDto)
    {
        var guid = Guid.NewGuid();
        try
        {
            Logger.LogInformation("Login request");
            await Transaction.BeginTransactionAsync(guid);

            var result = await authService.LoginAsync(loginDto, ApiKey);
            if (!result.Success)
            {
                await Transaction.RollbackTransactionAsync(guid, "Bad request");
                return BadRequest(result);
            }

            await Transaction.CommitTransactionAsync(guid);

            AppendCookie(TokenTypeList.RefreshToken, result.Data!.RefreshToken);
            return Ok(result.Data);
        }
        catch (Exception e)
        {
            await Transaction.RollbackTransactionAsync(guid, e.Message);
            return Error(e);
        }
    }

    [HttpPost]
    [Route("refresh")]
    [ProducesResponseType(typeof(JwtToken), StatusCodes.Status200OK)]
    public async Task<IActionResult> RefreshTokenAsync()
    {
        try
        {
            Logger.LogInformation("Refresh request");
            await Transaction.BeginTransactionAsync(UserId);

            var refreshToken = Request.Cookies[TokenTypeList.RefreshToken];
            if (string.IsNullOrEmpty(refreshToken)) throw new Exception("Invalid refresh token");

            var result = await authService.RefreshAsync(refreshToken, ApiKey);
            if (!result.Success)
            {
                await Transaction.RollbackTransactionAsync(UserId, "Bad request");
                return BadRequest(result);
            }

            await Transaction.CommitTransactionAsync(UserId);

            AppendCookie(TokenTypeList.RefreshToken, result.Data!.RefreshToken);
            return Ok(result.Data);
        }
        catch (Exception e)
        {
            return Error(e);
        }
    }

    [HttpPost]
    [Route("logout")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> LogoutAsync()
    {
        try
        {
            Logger.LogInformation("Logout request");
            await Transaction.BeginTransactionAsync(UserId);

            var refreshToken = Request.Cookies[TokenTypeList.RefreshToken];
            if (string.IsNullOrEmpty(refreshToken)) throw new Exception("Invalid refresh token");

            var result = await authService.LogoutAsync(refreshToken, ApiKey);
            if (!result.Success)
            {
                await Transaction.RollbackTransactionAsync(UserId, "Bad request");
                return BadRequest(result);
            }

            await Transaction.CommitTransactionAsync(UserId);

            Response.Cookies.Delete(TokenTypeList.RefreshToken);
            return Ok("User logged out");
        }
        catch (Exception e)
        {
            return Error(e);
        }
    }
}