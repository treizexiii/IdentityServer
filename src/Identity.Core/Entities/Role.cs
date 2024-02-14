using Microsoft.AspNetCore.Identity;

namespace Identity.Core.Entities;

public class Role
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string NormalizedName { get; set; } = string.Empty;
    public string ConcurrencyStamp { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
    public virtual ICollection<RoleClaims> Claims { get; set; } = new List<RoleClaims>();
}

public static class RolesList
{
    private static readonly List<Role> Roles =
    [
        new Role
        {
            Name = "SuperAdmin", NormalizedName = "SUPERADMIN", Id = Guid.Parse("1ab469f1-0140-41c9-9054-b16f5a8debd9"),
            ConcurrencyStamp = "1ab469f1-0140-41c9-9054-b16f5a8debd9"
        },
        new Role
        {
            Name = "Admin", NormalizedName = "ADMIN", Id = Guid.Parse("f465e822-e04e-47fa-a855-18ac37a93a35"),
            ConcurrencyStamp = "f465e822-e04e-47fa-a855-18ac37a93a35"
        },
        new Role
        {
            Name = "User", NormalizedName = "USER", Id = Guid.Parse("e4c8ab0c-386e-49b7-a27f-d28d52df8fb6"),
            ConcurrencyStamp = "e4c8ab0c-386e-49b7-a27f-d28d52df8fb6"
        }
    ];

    public static IEnumerable<Role> GetRoles()
    {
        return Roles;
    }

    public static Role GetRole(string roleName)
    {
        return Roles.First(r => r.Name == roleName);
    }

    public const string SuperAdmin = "SuperAdmin";
    public const string Admin = "Admin";
    public const string User = "User";
}