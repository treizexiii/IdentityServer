using Identity.Core.Entities;
using Identity.Core.Tools;
using Identity.Services.Auth;
using Identity.Wrappers.Dto;
using Microsoft.AspNetCore.Mvc;
using Tools.TransactionsManager;

namespace Identity.Server.Controllers._1._0;

[ApiController]
[Route("api/[controller]")]
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
    public async Task<IActionResult> RegisterAsync(RegisterDto registerDto)
    {
        try
        {
            Logger.LogInformation("Register request");
            var guid = Guid.NewGuid();
            await Transaction.BeginTransactionAsync(guid);

            var result = await authService.RegisterAsync(registerDto, RolesList.User);
            if (!result.Success)
            {
                await Transaction.RollbackTransactionAsync(UserId, "Bad request");
                return BadRequest(result as ServiceResult<ProblemsMessage>, UserId);
            }

            await Transaction.CommitTransactionAsync(guid);
            return Ok("User registered");
        }
        catch (Exception e)
        {
            await Transaction.RollbackTransactionAsync(UserId, e.Message);
            return Error(e);
        }
    }

    [HttpPost]
    [Route("login")]
    public async Task<IActionResult> LoginAsync(LoginDto loginDto)
    {
        try
        {
            Logger.LogInformation("Login request");
            var guid = Guid.NewGuid();
            await Transaction.BeginTransactionAsync(guid);

            var result = await authService.LoginAsync(loginDto);
            if (!result.Success)
            {
                await Transaction.RollbackTransactionAsync(UserId, "Bad request");
                return BadRequest(result as ServiceResult<ProblemsMessage>, UserId);
            }
            await Transaction.CommitTransactionAsync(guid);

            var accessToken = result as ServiceResult<JwtToken>;

            AppendCookie(TokenTypeList.RefreshToken, accessToken.Data.RefreshToken);
            return Ok(accessToken.Data);
        }
        catch (Exception e)
        {
            await Transaction.RollbackTransactionAsync(UserId, e.Message);
            return Error(e);
        }
    }

    [HttpPost]
    [Route("refresh")]
    public async Task<IActionResult> RefreshTokenAsync()
    {
        try
        {
            Logger.LogInformation("Refresh request");
            var guid = Guid.NewGuid();
            await Transaction.BeginTransactionAsync(guid);

            var refreshToken = Request.Cookies[TokenTypeList.RefreshToken];
            if (string.IsNullOrEmpty(refreshToken))
            {
                throw new Exception("Invalid refresh token");
            }

            var result = await authService.RefreshAsync(refreshToken);
            if (!result.Success)
            {
                await Transaction.RollbackTransactionAsync(UserId, "Bad request");
                return BadRequest(result as ServiceResult<ProblemsMessage>, UserId);
            }

            var accessToken = result as ServiceResult<JwtToken>;

            await Transaction.CommitTransactionAsync(guid);

            AppendCookie(TokenTypeList.RefreshToken, accessToken.Data.RefreshToken);
            return Ok(accessToken.Data);
        }
        catch (Exception e)
        {
            return Error(e);
        }
    }

    [HttpPost]
    [Route("logout")]
    public async Task<IActionResult> LogoutAsync()
    {
        try
        {
            Logger.LogInformation("Logout request");
            await Transaction.BeginTransactionAsync(UserId);

            var refreshToken = Request.Cookies[TokenTypeList.RefreshToken];
            if (string.IsNullOrEmpty(refreshToken))
            {
                throw new Exception("Invalid refresh token");
            }

            var result = await authService.LogoutAsync(refreshToken);
            if (!result.Success)
            {
                await Transaction.RollbackTransactionAsync(UserId, "Bad request");
                return BadRequest(result as ServiceResult<ProblemsMessage>, UserId);
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