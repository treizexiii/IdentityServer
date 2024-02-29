using Identity.Services.Factories;
using Identity.Wrappers.Dto;

namespace Identity.Services.Auth;

public interface IAuthService
{
    Task<ServiceResult> RegisterAsync(RegisterDto registerDto, string roleName);
    Task<ServiceResult<JwtToken>> LoginAsync(LoginDto loginDto, string apiKey);
    Task<ServiceResult<JwtToken>> RefreshAsync(string? refreshToken, string apiKey);
    Task<ServiceResult> LogoutAsync(string refreshToken, string apiKey);
}