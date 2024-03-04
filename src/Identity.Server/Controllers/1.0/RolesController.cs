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
[Route("api/v{version:apiVersion}/apps/{appId:guid}/[controller]")]
[Produces("application/json")]
public class RolesController(
    ILogger<RolesController> logger,
    ITransactionManager transaction,
    IHttpContextAccessor contextAccessor,
    IAdminService adminService)
    : IdentityControllerBase(logger, transaction, contextAccessor)
{
    [HttpGet]
    [Route("")]
    [Authorize(Roles = RolesList.Admin)]
    public async Task<IActionResult> GetRolesAsync(Guid appId)
    {
        try
        {
            Logger.LogInformation("Method:GetRolesAsync");
            await Transaction.BeginTransactionAsync(UserId);

            var result = await adminService.GetRolesAsync(UserId, appId);
            if (!result.Success)
            {
                await Transaction.RollbackTransactionAsync(UserId, result.Message);
                return BadRequest(result);
            }

            await Transaction.CommitTransactionAsync(UserId);
            return Ok("Roles retrieved");
        }
        catch (Exception e)
        {
            await Transaction.RollbackTransactionAsync(UserId, e.Message);
            return Error(e);
        }
    }

    [HttpPut]
    [Route("")]
    [Authorize(Roles = RolesList.Admin)]
    public async Task<IActionResult> CreateRoleAsync(Guid appId, AppRoleCreateDto role)
    {
        try
        {
            Logger.LogInformation("Method:CreateRoleAsync");
            await Transaction.BeginTransactionAsync(UserId);

            var result = await adminService.CreateRoleAsync(UserId, appId, role);
            if (!result.Success)
            {
                await Transaction.RollbackTransactionAsync(UserId, result.Message);
                return BadRequest(result);
            }

            await Transaction.CommitTransactionAsync(UserId);
            return Ok(result.Message);
        }
        catch (Exception e)
        {
            await Transaction.RollbackTransactionAsync(UserId, e.Message);
            return Error(e);
        }
    }

    [HttpDelete]
    [Route("{roleId:guid}")]
    [Authorize(Roles = RolesList.Admin)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> DeleteRoleAsync(Guid appId, Guid roleId)
    {
        try
        {
            Logger.LogInformation("Method:DeleteRoleAsync");
            await Transaction.BeginTransactionAsync(UserId);

            var result = await adminService.DeleteRoleAsync(UserId,appId, roleId);
            if (!result.Success)
            {
                await Transaction.RollbackTransactionAsync(UserId, result.Message);
                return BadRequest(result);
            }

            await Transaction.CommitTransactionAsync(UserId);
            return Ok("Role deleted");
        }
        catch (Exception e)
        {
            await Transaction.RollbackTransactionAsync(UserId, e.Message);
            return Error(e);
        }
    }
}