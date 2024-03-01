using System.Text.Json;
using Identity.Core.Entities;
using Identity.Services.Admin;
using Identity.Services.Auth;
using Identity.Wrappers.Dto;
using Microsoft.Extensions.Logging;
using Tools.TransactionsManager;

namespace Identity.Migrator;

public class SuperAdminBuilder(
    ITransactionManager transactionManager,
    ILogger<SuperAdminBuilder> logger,
    IAdminService adminService,
    IAuthService authService)
{
    public async Task Create()
    {
        try
        {
            // load data from json
            var json = File.ReadAllText("datas/su.json");
            var data = JsonSerializer.Deserialize<RegisterAppDto>(json);
            if (data == null)
            {
                throw new Exception("Super admin data not found");
            }

            await transactionManager.BeginTransactionAsync(Guid.Empty);
            logger.LogInformation("Create root app");
            var result = await adminService.CreateAppAsync(data);
            if (!result.Success)
            {
                throw new Exception("Super admin app creation failed");
            }
            if (result.Data == null)
            {
                throw new Exception("Super admin app creation failed");
            }
            logger.LogInformation("ApiKey: {DataApiKey}", result.Data.ApiKey);
            logger.LogInformation("Secret: {DataSecret}", result.Data.Secret);

            logger.LogInformation("Create super admin");
            var su = await authService.RegisterAsync(data.ToRegisterRequest(result.Data.ApiKey), RolesList.SuperAdmin);
            if (!su.Success)
            {
                throw new Exception("Super admin creation failed");
            }

            logger.LogInformation("Super admin created");

            await transactionManager.CommitTransactionAsync(Guid.Empty);
        }
        catch (Exception e)
        {
            await transactionManager.RollbackTransactionAsync(Guid.Empty, e.Message);
            logger.LogError(e, "Super admin creation failed");
        }
    }
}