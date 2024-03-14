using Asp.Versioning;
using Identity.Core.Entities;
using Identity.Services.Admin;
using Identity.Wrappers.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tools.TransactionsManager;

namespace Identity.Server.Controllers._1._0;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Produces("application/json")]
public class AppsController(
    ILogger<AppsController> logger,
    ITransactionManager transaction,
    IHttpContextAccessor contextAccessor,
    IAdminService adminService)
    : IdentityControllerBase(logger, transaction, contextAccessor)
{
    [HttpGet]
    [Route("")]
    [Authorize(Roles = RolesList.Admin)]
    [ProducesResponseType(typeof(IEnumerable<AppViewDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAppsAsync()
    {
        try
        {
            Logger.LogInformation("Get apps request");
            await Transaction.BeginTransactionAsync(UserId);

            var result = await adminService.GetAppsAsync(UserId);
            if (!result.Success)
            {
                await Transaction.RollbackTransactionAsync(UserId, "Bad request");
                return BadRequest(result);
            }

            await Transaction.CommitTransactionAsync(UserId);
            return Ok(result.Data);
        }
        catch (Exception e)
        {
            await Transaction.RollbackTransactionAsync(UserId, e.Message);
            return Error(e);
        }
    }

    [HttpGet]
    [Route("{id:guid}")]
    [Authorize(Roles = RolesList.Admin)]
    [ProducesResponseType(typeof(AppViewDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAppAsync(Guid id)
    {
        try
        {
            Logger.LogInformation("Get app request");
            await Transaction.BeginTransactionAsync(UserId);

            var result = await adminService.GetAppAsync(UserId, id);
            if (!result.Success)
            {
                await Transaction.RollbackTransactionAsync(UserId, "Bad request");
                return BadRequest(result);
            }

            await Transaction.CommitTransactionAsync(UserId);
            return Ok(result.Data);
        }
        catch (Exception e)
        {
            await Transaction.RollbackTransactionAsync(UserId, e.Message);
            return Error(e);
        }
    }

    [HttpPost]
    [Route("{id:guid}/update-configuration")]
    [Authorize(Roles = RolesList.Admin)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateConfigurationAsync(Guid id, AppConfigDto configuration)
    {
        try
        {
            Logger.LogInformation("Update app configuration request");
            await Transaction.BeginTransactionAsync(UserId);

            var result = await adminService.UpdateAppAsync(UserId, id, configuration);
            if (!result.Success)
            {
                await Transaction.RollbackTransactionAsync(UserId, "Bad request");
                return BadRequest(result);
            }

            await Transaction.CommitTransactionAsync(UserId);
            return Ok("App configuration updated");
        }
        catch (Exception e)
        {
            await Transaction.RollbackTransactionAsync(UserId, e.Message);
            return Error(e);
        }
    }
}