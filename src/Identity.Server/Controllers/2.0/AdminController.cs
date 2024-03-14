using Asp.Versioning;
using Identity.Services.Admin;
using Identity.Services.Auth;
using Microsoft.AspNetCore.Mvc;
using Tools.TransactionsManager;

namespace Identity.Server.Controllers._2._0;

[ApiController]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Produces("application/json")]
public class AdminController(
    ILogger<IdentityControllerBase> logger,
    ITransactionManager transaction,
    IHttpContextAccessor contextAccessor,
    IAdminService adminService,
    IAuthService authService)
    : IdentityControllerBase(logger, transaction, contextAccessor)
{
    [HttpGet]
    public async Task<IActionResult> HelloWorld()
    {
        return Ok("Hello World");
    }
}