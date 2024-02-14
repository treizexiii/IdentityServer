using System.Data;
using System.Security.Claims;
using Identity.Core.Entities;
using Identity.Core.Managers;
using Identity.Core.Repositories;
using Identity.Core.Tools;
using Identity.Wrappers.Dto;

namespace Identity.Services.Auth;

internal class AuthService(
    IUsersRepository userRepository,
    ITokenManager tokenManager,
    IAppsRepository appsRepository,
    IAuthOptions authOptions)
    : IAuthService
{
    public async Task RegisterAsync(RegisterDto registerDto, string roleName)
    {
        if (await userRepository.IsExistAsync(registerDto.Email))
        {
            throw new DataException("User already exists");
        }

        var utc = DateTime.UtcNow;

        var hashedPassword = DataHasher.Hash(registerDto.Password);

        var user = new User();
        user.Id = Guid.NewGuid();
        user.Email = registerDto.Email;
        user.NormalizedEmail = registerDto.Email.ToUpper();
        user.UserName = registerDto.Email;
        user.NormalizedUserName = registerDto.Email.ToUpper();
        user.SecurityStamp = Guid.NewGuid().ToString();
        user.ConcurrencyStamp = Guid.NewGuid().ToString();
        user.EmailConfirmed = false;
        user.PasswordHash = hashedPassword.hash;
        user.PasswordSalt = hashedPassword.salt;
        user.CreatedAt = utc;
        user.ActiveAt = utc;
        user.Active = true;

        var app = await appsRepository.GetAppAsync(registerDto.AppKey);
        if (app == null)
        {
            throw new DataException("Provider not found");
        }

        var role = await userRepository.GetRoleAsync(roleName);
        if (role == null)
        {
            throw new DataException("Role not found");
        }

        user.UserRole = new UserRole
        {
            UserId = user.Id,
            RoleId = role.Id,
            CreatedAt = utc
        };

        user.UserApps.Add(new UserApp
        {
            UserId = user.Id,
            AppId = app.Id,
            CreatedAt = utc
        });

        await userRepository.AddUserAsync(user);
    }

    public async Task<JwtToken> LoginAsync(LoginDto loginDto)
    {
        var user = await userRepository.GetUserAsync(loginDto.Email);
        if (user == null)
        {
            throw new Exception("Credentials error");
        }

        if (user.Active == false)
        {
            throw new Exception("Credentials error");
        }

        // if (user.UserLogins.All(l => l.LoginProvider != loginDto.AppKey))
        // {
        //     throw new Exception("Credentials error");
        // }

        if (!DataHasher.ControlHash(loginDto.Password, user.PasswordHash, user.PasswordSalt))
        {
            throw new Exception("Credentials error");
        }

        string? appKey = null;
        if (loginDto.AppKey != null)
        {
            var app = await appsRepository.GetAppAsync(loginDto.AppKey);
            if (app == null)
            {
                throw new DataException("Provider not found");
            }

            appKey = app.Key;
        }

        var (accessToken, refreshToken) = await ProduceJwtToken(user, appKey);

        return new JwtToken
        {
            TokenType = TokenTypeList.BearToken,
            Token = accessToken,
            RefreshToken = refreshToken
        };
    }

    public async Task<JwtToken> RefreshAsync(string? refreshToken)
    {
        if (string.IsNullOrEmpty(refreshToken))
            throw new UnauthorizedAccessException("Invalid token");

        var tokenBuilder = new JwtTokenBuilder(authOptions.Issuer, authOptions.Audience, authOptions.Key);
        var claims = tokenBuilder.ControlToken(refreshToken);

        var username = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
        if (string.IsNullOrEmpty(username))
            throw new UnauthorizedAccessException("Invalid token");

        var user = await userRepository.GetUserAsync(username);
        if (user is null)
            throw new UnauthorizedAccessException("Invalid token");

        var token = await tokenManager.GetLastTokenAsync(user.Id,
            LoginProviders.Internal,
            TokenTypeList.RefreshToken);
        if (token is null)
            throw new UnauthorizedAccessException("Invalid token");

        if (token.Value != refreshToken)
            throw new UnauthorizedAccessException("Invalid token");

        var appKey = claims.FirstOrDefault(c => c.Type == ClaimTypes.Spn)?.Value;
        var (accessToken, newRefreshToken) = await ProduceJwtToken(user, appKey);

        return new JwtToken
        {
            TokenType = TokenTypeList.BearToken,
            Token = accessToken,
            RefreshToken = newRefreshToken
        };
    }

    public async Task LogoutAsync(string refreshToken)
    {
        if (string.IsNullOrEmpty(refreshToken))
            throw new UnauthorizedAccessException("Invalid token");

        var tokenBuilder = new JwtTokenBuilder(authOptions.Issuer, authOptions.Audience, authOptions.Key);
        var claims = tokenBuilder.ControlToken(refreshToken);

        var username = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
        if (string.IsNullOrEmpty(username))
            throw new UnauthorizedAccessException("Invalid token");

        var user = await userRepository.GetUserAsync(username);
        if (user is null)
            throw new UnauthorizedAccessException("Invalid token");

        var token = await tokenManager.GetLastTokenAsync(user.Id,
            LoginProviders.Internal,
            TokenTypeList.RefreshToken);
        if (token is null)
            throw new UnauthorizedAccessException("Invalid token");

        if (token.Value != refreshToken)
            throw new UnauthorizedAccessException("Invalid token");

        await tokenManager.RevokeTokenAsync(token);
    }

    private async Task<(string jwt, string refresh)> ProduceJwtToken(User user, string? appKey)
    {
        var jwtBuilder = new JwtTokenBuilder(authOptions.Issuer, authOptions.Audience, authOptions.Key)
            .AddUser(user)
            .AddRole(user.UserRole.Role);

        if (appKey != null)
        {
            jwtBuilder.AddProvider(appKey);
        }

        var jwtToken = jwtBuilder.BuildJwtToken();
        var refreshToken = jwtBuilder.BuildRefreshToken(jwtToken);


        await tokenManager.GenerateTokenAsync(
            user.Id,
            LoginProviders.Internal,
            TokenTypeList.RefreshToken,
            refreshToken,
            null);

        return (jwtToken, refreshToken);
    }
}