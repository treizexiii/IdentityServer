using Identity.Core.Entities;
using Identity.Wrappers.Dto;

namespace Identity.Services.Builders;

public class AppUserDtoBuilder(User user)
{
    public AppUserDto Build()
    {
        var role = user.UserRoles.FirstOrDefault()?.Role.Name;
        return new AppUserDto(
            user.Id,
            user.Email ?? string.Empty,
            user.UserName ?? string.Empty,
            role ?? string.Empty,
            user.PhoneNumber ?? string.Empty,
            user.EmailConfirmed,
            user.Active,
            user.CreatedAt.DateTime,
            user.DeactivatedAt?.DateTime);
    }
}