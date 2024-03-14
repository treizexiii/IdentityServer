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
[Route("api/v{version:apiVersion}/apps/{id:guid}/[controller]")]
[Produces("application/json")]
public class UsersController(
    ILogger<RolesController> logger,
    ITransactionManager transaction,
    IHttpContextAccessor contextAccessor,
    IAdminService adminService)
    : IdentityControllerBase(logger, transaction, contextAccessor)
{
    [HttpGet]
    [Route("")]
    [Authorize(Roles = RolesList.Admin)]
    [ProducesResponseType(typeof(IEnumerable<AppUserDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAppUsersAsync(Guid id)
    {
        try
        {
            Logger.LogInformation("Get app users request");
            await Transaction.BeginTransactionAsync(UserId);

            var result = await adminService.GetAppUsersAsync(UserId, id, RolesList.All);
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
    [Route("{userId:guid}")]
    [Authorize(Roles = RolesList.Admin)]
    [ProducesResponseType(typeof(AppUserDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAppUserAsync(Guid id, Guid userId)
    {
        try
        {
            Logger.LogInformation("Get app users request");
            await Transaction.BeginTransactionAsync(UserId);

            // var result = await adminService.GetAppUsersAsync(UserId, id, RolesList.All);
            // if (!result.Success)
            // {
            //     await Transaction.RollbackTransactionAsync(UserId, "Bad request");
            //     return BadRequest(result);
            // }

            await Transaction.CommitTransactionAsync(UserId);
            return Ok("App users retrieved");
        }
        catch (Exception e)
        {
            await Transaction.RollbackTransactionAsync(UserId, e.Message);
            return Error(e);
        }
    }

    [HttpPut]
    [Route("{userId:guid}/grant-role:{role}")]
    [Authorize(Roles = RolesList.Admin)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GrantRoleAsync(Guid id, Guid userId, Role role)
    {
        try
        {
            Logger.LogInformation("Grant role request");
            await Transaction.BeginTransactionAsync(UserId);

            // var result = await adminService.GrantRoleAsync(id, userId, role);
            // if (!result.Success)
            // {
            //     await Transaction.RollbackTransactionAsync(UserId, "Bad request");
            //     return BadRequest(result);
            // }

            await Transaction.CommitTransactionAsync(UserId);
            return Ok("Role granted");
        }
        catch (Exception e)
        {
            await Transaction.RollbackTransactionAsync(UserId, e.Message);
            return Error(e);
        }
    }

    [HttpPut]
    [Route("{userId:guid}/revoke-role:{role}")]
    [Authorize(Roles = RolesList.Admin)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> RevokeRoleAsync(Guid id, Guid userId, Role role)
    {
        try
        {
            Logger.LogInformation("Revoke role request");
            await Transaction.BeginTransactionAsync(UserId);

            // var result = await adminService.RevokeRoleAsync(id, userId, role);
            // if (!result.Success)
            // {
            //     await Transaction.RollbackTransactionAsync(UserId, "Bad request");
            //     return BadRequest(result);
            // }

            await Transaction.CommitTransactionAsync(UserId);
            return Ok("Role revoked");
        }
        catch (Exception e)
        {
            await Transaction.RollbackTransactionAsync(UserId, e.Message);
            return Error(e);
        }
    }

}