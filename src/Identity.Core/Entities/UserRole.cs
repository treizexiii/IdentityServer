using Microsoft.AspNetCore.Identity;

namespace Identity.Core.Entities;

public class UserRole
{
    public Guid RoleId { get; set; }
    public virtual Role Role { get; set; } = null!;
    public Guid UserId { get; set; }
    public virtual User User { get; set; } = null!;
    public DateTimeOffset CreatedAt { get; set; }
}