namespace Identity.Core.Entities;

public class Role
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string NormalizedName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ConcurrencyStamp { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
    public virtual ICollection<RoleClaims> Claims { get; set; } = new List<RoleClaims>();
    public bool IsSystemDefault { get; set; }
    public bool CouldBeDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public Guid AppId { get; set; }
}

public static class RolesList
{
    public const string SuperAdmin = "SuperAdmin";
    public const string Admin = "Admin";
    public const string User = "User";
    public const string All = "*";

    private static readonly List<Role> Roles =
    [
        new Role
        {
            Id = Guid.Parse("1ab469f1-0140-41c9-9054-b16f5a8debd9"),
            Name = "SuperAdmin",
            NormalizedName = "SUPERADMIN",
            ConcurrencyStamp = "1ab469f1-0140-41c9-9054-b16f5a8debd9",
            IsSystemDefault = true,
            CouldBeDeleted = false,
            AppId = Guid.Parse("00000000-0000-0000-0000-000000000000")
        },
        new Role
        {
            Id = Guid.Parse("f465e822-e04e-47fa-a855-18ac37a93a35"),
            Name = "Admin",
            NormalizedName = "ADMIN",
            ConcurrencyStamp = "f465e822-e04e-47fa-a855-18ac37a93a35",
            IsSystemDefault = true,
            CouldBeDeleted = false,
            AppId = Guid.Parse("00000000-0000-0000-0000-000000000000")
        },
        new Role
        {
            Id = Guid.Parse("e4c8ab0c-386e-49b7-a27f-d28d52df8fb6"),
            Name = "User",
            NormalizedName = "USER",
            ConcurrencyStamp = "e4c8ab0c-386e-49b7-a27f-d28d52df8fb6",
            IsSystemDefault = true,
            CouldBeDeleted = false,
            AppId = Guid.Parse("00000000-0000-0000-0000-000000000000")
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
}