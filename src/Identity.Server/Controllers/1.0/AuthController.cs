using Identity.Core.Entities;
using Identity.Services.Auth;
using Identity.Wrappers.Dto;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Tools.TransactionsManager;

namespace Identity.Server.Controllers._1._0;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController : IdentityControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(
        ILogger<AuthController> logger,
        ITransactionManager transaction,
        IHttpContextAccessor contextAccessor,
        IAuthService authService)
        : base(logger, transaction, contextAccessor)
    {
        _authService = authService;
    }

    [HttpPost]
    [Route("register")]
    public async Task<IActionResult> RegisterAsync(RegisterDto registerDto)
    {
        try
        {
            Logger.LogInformation("Register request");
            var guid = Guid.NewGuid();
            await Transaction.BeginTransactionAsync(guid);

            await _authService.RegisterAsync(registerDto, RolesList.User);

            await Transaction.CommitTransactionAsync(guid);
            return Ok("User registered");
        }
        catch (Exception e)
        {
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

            var accessToken = await _authService.LoginAsync(loginDto);
            if (accessToken == null)
            {
                throw new Exception("Invalid credentials");
            }

            await Transaction.CommitTransactionAsync(guid);

            AppendCookie(TokenTypeList.RefreshToken, accessToken.RefreshToken);
            return Ok(accessToken);
        }
        catch (Exception e)
        {
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

            var accessToken = await _authService.RefreshAsync(refreshToken);
            if (accessToken == null)
            {
                throw new Exception("Invalid refresh token");
            }

            await Transaction.CommitTransactionAsync(guid);

            AppendCookie(TokenTypeList.RefreshToken, accessToken.RefreshToken);
            return Ok(accessToken);
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

            await _authService.LogoutAsync(refreshToken);

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