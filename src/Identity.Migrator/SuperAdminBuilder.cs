using Identity.Core.Entities;
using Identity.Core.Tools;
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
    private static readonly RegisterAppDto Register = new(
        "superadmin@root.fr",
        "Superadmin123!",
        "root",
        "Root access application");

    public async Task Create()
    {
        try
        {
            await transactionManager.BeginTransactionAsync(Guid.Empty);
            logger.LogInformation("Create root app");
            var result = await adminService.CreateAppAsync(Register);
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
            var su = await authService.RegisterAsync(Register.ToRegisterRequest(result.Data.ApiKey), RolesList.SuperAdmin);
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

    public static User CreateSuperUser()
    {
        var role = RolesList.GetRole(RolesList.SuperAdmin);
        var su = new User();
        su.Id = Guid.Parse("0b55857c-d9d2-4fb5-87a4-2c9971686426");
        su.UserName = "superadmin";
        su.NormalizedUserName = "SUPERADMIN";
        su.Email = "superadmin@localhost.fr";
        su.NormalizedEmail = "superadmin@localhost.fr".Normalize();
        su.EmailConfirmed = true;

        var password = DataHasher.Hash("superadmin");
        su.PasswordHash = password.hash;
        su.PasswordSalt = password.salt;
        su.SecurityStamp = Guid.NewGuid().ToString();
        su.ConcurrencyStamp = Guid.NewGuid().ToString();
        su.CreatedAt = DateTime.UtcNow;
        su.ActiveAt = DateTime.UtcNow;
        su.Active = true;

        su.UserRoles.Add(new UserRole
        {
            UserId = su.Id,
            RoleId = role.Id,
            CreatedAt = DateTime.UtcNow
        });

        return su;
    }
}