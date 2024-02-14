using Identity.Wrappers.Dto;

namespace Identity.Services.Auth;

public interface IAuthService
{
    Task<JwtToken> LoginAsync(LoginDto loginDto);
    Task RegisterAsync(RegisterDto registerDto, string roleName);
    Task<JwtToken> RefreshAsync(string? refreshToken);
    Task LogoutAsync(string refreshToken);
}