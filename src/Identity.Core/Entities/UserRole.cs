using System.Security.Claims;

namespace Identity.Core.Entities;

public class UserRole
{
    public Guid RoleId { get; set; }
    public virtual Role Role { get; set; } = null!;
    public Guid UserId { get; set; }
    public virtual User User { get; set; } = null!;
    public Guid AppId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public Claim ToClaim()
    {
        return new Claim(ClaimTypes.Role, Role.Name);
    }
}