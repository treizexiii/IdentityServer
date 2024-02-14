namespace Identity.Core.Entities;

public class User
{
    public Guid Id { get; set; }
    public string? UserName { get; set; }
    public string? NormalizedUserName { get; set; }
    public string? Email { get; set; }
    public bool EmailConfirmed { get; set; }
    public string? NormalizedEmail { get; set; }
    public string? PhoneNumber { get; set; }
    public bool PhoneNumberConfirmed { get; set; }
    public string? SecurityStamp { get; set; } // change on credential change
    public string? ConcurrencyStamp { get; set; } // change on entity change
    public bool LockoutEnabled { get; set; }
    public int AccessFailedCount { get; set; }
    public DateTimeOffset? LockoutEnd { get; set; }
    public bool TwoFactorEnabled { get; set; }
    public byte[] PasswordHash { get; set; } = Array.Empty<byte>();
    public byte[] PasswordSalt { get; set; } = Array.Empty<byte>();
    public bool Active { get; set; } = false;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? ActiveAt { get; set; }
    public DateTimeOffset? DeactivatedAt { get; set; }
    public virtual UserRole UserRole { get; set; } = new();
    public virtual ICollection<UserLogin> UserLogins { get; set; } = new List<UserLogin>();
    public virtual ICollection<UserClaim> UserClaims { get; set; } = new List<UserClaim>();
    public virtual ICollection<UserApp> UserApps { get; set; } = new List<UserApp>();
}