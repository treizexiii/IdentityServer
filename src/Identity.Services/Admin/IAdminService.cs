using Identity.Core.Entities;
using Identity.Services.Factories;
using Identity.Wrappers.Dto;

namespace Identity.Services.Admin;

public interface IAdminService
{
    Task<ServiceResult<IEnumerable<AppViewDto>>> GetAppsAsync(Guid ownerId);
    Task<ServiceResult<AppViewDto>> GetAppAsync(Guid owner, Guid appId);
    Task<ServiceResult<AppDto>> CreateAppAsync(RegisterAppDto registerAppDto);
    Task<ServiceResult> UpdateAppAsync(Guid owner, Guid appId, AppConfigDto configuration);
    Task<ServiceResult<IEnumerable<AppUserDto>>> GetAppUsersAsync(Guid owner, Guid id, string role);
}

public class AppDtoBuilder
{
    private App _app;

    public AppDtoBuilder(App app)
    {
        _app = app;
    }

    public AppViewDto Build()
    {
        var config = new AppConfigDto(
            _app.Id,
            _app.Configuration.TokenExpiration,
            _app.Configuration.RefreshTokenExpiration,
            _app.Configuration.Issuer,
            _app.Configuration.Audience,
            _app.Configuration.UpdatedAt.DateTime);

        return new AppViewDto(_app.Id, _app.Name, _app.Description, _app.ApiKey, _app.CreatedAt.DateTime, config);
    }
}

public class AppUserDtoBuilder
{
    private readonly User _user;

    public AppUserDtoBuilder(User user)
    {
        _user = user;
    }

    public AppUserDto Build()
    {
        var role = _user.UserRoles.FirstOrDefault()?.Role.Name;
        return new AppUserDto(
            _user.Id,
            _user.Email ?? string.Empty,
            _user.UserName ?? string.Empty,
            role ?? string.Empty,
            _user.PhoneNumber ?? string.Empty,
            _user.EmailConfirmed,
            _user.Active,
            _user.CreatedAt.DateTime,
            _user.DeactivatedAt?.DateTime);
    }
}