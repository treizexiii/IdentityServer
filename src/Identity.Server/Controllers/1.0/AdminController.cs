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
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> RegisterAppAsync(RegisterAppDto registerAppDto)
    {
        try
        {
            Logger.LogInformation("Register provider request");
            var guid = Guid.NewGuid();
            await Transaction.BeginTransactionAsync(guid);

            var appKey = await adminService.CreateAppAsync(registerAppDto);
            await authService.RegisterAsync(registerAppDto.ToRegisterRequest(appKey), RolesList.Admin);

            await Transaction.CommitTransactionAsync(guid);
            return Ok("Provider registered");
        }
        catch (Exception e)
        {
            return Error(e);
        }
    }
}