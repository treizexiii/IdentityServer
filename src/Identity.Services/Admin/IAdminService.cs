using Identity.Services.Factories;
using Identity.Wrappers.Dto;

namespace Identity.Services.Admin;

public interface IAdminService
{
    Task<ServiceResult<AppDto>> CreateAppAsync(RegisterAppDto registerAppDto);
}