using Identity.Core.Entities;
using Identity.Core.Tools;

namespace Identity.Migrator;

public static class SuperAdminBuilder
{
    public static User Create()
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

        su.UserRole = new UserRole
        {
            UserId = su.Id,
            RoleId = role.Id,
            CreatedAt = DateTime.UtcNow
        };

        return su;
    }
}