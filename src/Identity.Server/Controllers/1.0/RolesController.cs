using Asp.Versioning;
using Identity.Core.Entities;
using Identity.Services.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tools.TransactionsManager;

namespace Identity.Server.Controllers._1._0;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/apps/{id:guid}/[controller]")]
[Produces("application/json")]
public class RolesController(
    ILogger<RolesController> logger,
    ITransactionManager transaction,
    IHttpContextAccessor contextAccessor,
    IAdminService adminService)
    : IdentityControllerBase(logger, transaction, contextAccessor)
{
    [HttpPost]
    [Route("")]
    [Authorize(Roles = RolesList.Admin)]
    public async Task<IActionResult> CreateRoleAsync(Guid id, Role role)
    {
        try
        {
            Logger.LogInformation("Create role request");
            await Transaction.BeginTransactionAsync(UserId);

            // var result = await adminService.CreateRoleAsync(id, role);
            // if (!result.Success)
            // {
            //     await Transaction.RollbackTransactionAsync(UserId, "Bad request");
            //     return BadRequest(result);
            // }

            await Transaction.CommitTransactionAsync(UserId);
            return Ok("Role created");
        }
        catch (Exception e)
        {
            await Transaction.RollbackTransactionAsync(UserId, e.Message);
            return Error(e);
        }
    }

    [HttpGet]
    [Route("")]
    [Authorize(Roles = RolesList.Admin)]
    public async Task<IActionResult> GetRolesAsync(Guid id)
    {
        try
        {
            Logger.LogInformation("Get roles request");
            await Transaction.BeginTransactionAsync(UserId);

            // var result = await adminService.GetRolesAsync(id);
            // if (!result.Success)
            // {
            //     await Transaction.RollbackTransactionAsync(UserId, "Bad request");
            //     return BadRequest(result);
            // }

            await Transaction.CommitTransactionAsync(UserId);
            return Ok("Roles retrieved");
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
    public async Task<IActionResult> DeleteRoleAsync(Guid id, Guid roleId)
    {
        try
        {
            Logger.LogInformation("Delete role request");
            await Transaction.BeginTransactionAsync(UserId);

            // var result = await adminService.DeleteRoleAsync(id, roleId);
            // if (!result.Success)
            // {
            //     await Transaction.RollbackTransactionAsync(UserId, "Bad request");
            //     return BadRequest(result);
            // }

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