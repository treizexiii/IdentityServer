using Identity.Services.Factories;
using Identity.Wrappers.Dto;

namespace Identity.Services.Admin;

public interface IAdminService
{
    Task<ServiceResult<IEnumerable<AppViewDto>>> GetAppsAsync(Guid ownerId);
    Task<ServiceResult<AppViewDto>> GetAppAsync(Guid owner, Guid appId);
    Task<ServiceResult<AppDto>> CreateAppAsync(RegisterAppDto registerAppDto);
    Task<ServiceResult> UpdateAppAsync(Guid owner, Guid appId, AppConfigDto configuration);
    Task<ServiceResult<IEnumerable<AppRoleDto>>> GetRolesAsync(Guid owner, Guid appId);
    Task<ServiceResult> CreateRoleAsync(Guid userId, Guid appId, AppRoleCreateDto role);
    Task<ServiceResult> DeleteRoleAsync(Guid owner, Guid appId, Guid roleId);
    Task<ServiceResult<IEnumerable<AppUserDto>>> GetAppUsersAsync(Guid owner, Guid id, string role);
}