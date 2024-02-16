using Identity.Wrappers.Dto;

namespace Identity.Services.Auth;

public interface IAuthService
{
    Task<ServiceResult> LoginAsync(LoginDto loginDto);
    Task<ServiceResult> RegisterAsync(RegisterDto registerDto, string roleName);
    Task<ServiceResult> RefreshAsync(string? refreshToken);
    Task<ServiceResult> LogoutAsync(string refreshToken);
}