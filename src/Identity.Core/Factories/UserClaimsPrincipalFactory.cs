using System.Security.Claims;
using Identity.Core.Entities;
using Microsoft.AspNetCore.Identity;

namespace Identity.Core.Factories;

public class UserClaimsPrincipalFactory : IUserClaimsPrincipalFactory<User>
{
    public Task<ClaimsPrincipal> CreateAsync(User user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.UserName),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.UserRole.Role.Name)
        };

        foreach (var userClaim in user.UserClaims)
        {
            claims.Add(userClaim.ToClaim());
        }

        foreach (var role in user.UserRole.Role.Claims)
        {
            claims.Add(role.ToClaim());
        }

        var identity = new ClaimsIdentity(claims, "Identity.Application");
        return Task.FromResult(new ClaimsPrincipal(identity));
    }
}