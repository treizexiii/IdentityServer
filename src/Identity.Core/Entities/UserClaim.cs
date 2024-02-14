using System.Security.Claims;

namespace Identity.Core.Entities;

public class UserClaim
{
    public Guid Id { get; set; }
    public string ClaimType { get; set; } = string.Empty;
    public string ClaimValue { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public virtual User User { get; set; } = null!;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? RevokedAt { get; set; }

    public Claim ToClaim()
    {
        return new Claim(ClaimType ?? string.Empty, ClaimValue ?? string.Empty);
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