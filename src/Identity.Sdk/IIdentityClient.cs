using Identity.Wrappers.Dto;
using Identity.Wrappers.Messages;

namespace Identity.Sdk;

public interface IIdentityClient
{
    Task<ApiResponse> LoginAsync(LoginDto loginDto);
    Task<ApiResponse> RefreshTokenAsync();
    Task<ApiResponse> LogoutAsync();
    Task<ApiResponse> RegisterAsync(RegisterDto registerDto);
    Task<ApiResponse> RegisterAppAsync(RegisterAppDto registerAppDto);
}