using Identity.Core.Entities;
using Identity.Services.Admin;
using Identity.Services.Auth;
using Identity.Wrappers.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tools.TransactionsManager;

namespace Identity.Server.Controllers._1._0;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AdminController(
    ILogger<IdentityControllerBase> logger,
    ITransactionManager transaction,
    IHttpContextAccessor contextAccessor,
    IAdminService adminService,
    IAuthService authService)
    : IdentityControllerBase(logger, transaction, contextAccessor)
{
    [HttpPost]
    [Route("register-app")]
    [Authorize(Roles = RolesList.SuperAdmin)]
    public async Task<IActionResult> RegisterAppAsync(RegisterAppDto registerAppDto)
    {
        try
        {
            Logger.LogInformation("Register provider request");
            await Transaction.BeginTransactionAsync(UserId);

            var result = await adminService.CreateAppAsync(registerAppDto);
            if (result.Success is false)
            {
                await Transaction.RollbackTransactionAsync(UserId, result.Errors!);
                return BadRequest(result);
            }
            await authService.RegisterAsync(registerAppDto.ToRegisterRequest(result.Data!.ApiKey), RolesList.Admin);

            await Transaction.CommitTransactionAsync(UserId);
            return Ok("App registered");
        }
        catch (Exception e)
        {
            await Transaction.RollbackTransactionAsync(UserId, e.Message);
            return Error(e);
        }
    }
}