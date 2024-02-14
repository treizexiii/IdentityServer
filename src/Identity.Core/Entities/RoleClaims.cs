using System.Security.Claims;
using Microsoft.AspNetCore.Identity;

namespace Identity.Core.Entities;

public class RoleClaims
{
    public Guid Id { get; set; }
    public Guid RoleId { get; set; }
    public virtual Role Role { get; set; } = null!;
    public DateTimeOffset CreatedAt { get; set; }
    public string ClaimType { get; set; } = null!;
    public string ClaimValue { get; set; } = null!;

    public Claim ToClaim()
    {
        return new(ClaimType, ClaimValue);
    }

    public void InitializeFromClaim(Claim? other)
    {
        if (other == null)
        {
            return;
        }

        ClaimType = other.Type;
        ClaimValue = other.Value;
    }
}