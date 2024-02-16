using System.Security.Claims;
using Identity.Core.Configuration;
using Identity.Core.Entities;
using Identity.Core.Managers;
using Identity.Core.Repositories;
using Identity.Core.Tools;
using Identity.Wrappers.Dto;

namespace Identity.Services.Auth;

public class ServiceResult
{
    public bool Success { get; set; }
}

public class ServiceResult<T> : ServiceResult
{
    public T? Data { get; set; }
}

public static class ServiceResultFactory
{
    public static ServiceResult Ok() => new() { Success = true };
    public static ServiceResult Fail(string message) => new ServiceResult<ProblemsMessage>
        { Success = false, Data = new ProblemsMessage { Errors = { message } } };
}

public class ServiceResultFactory<T>
{
    public static ServiceResult<T> Ok(T data)
        => new() { Success = true, Data = data };
    public static ServiceResult<T> Fail(T data) => new() { Success = false, Data = data };
}

internal class AuthService(
    IUsersRepository userRepository,
    ITokenManager tokenManager,
    IAppsRepository appsRepository,
    IAuthOptions authOptions,
    IJwtOptions jwtOptions)
    : IAuthService
{
    public async Task<ServiceResult> RegisterAsync(RegisterDto registerDto, string roleName)
    {
        var errors = new DataControls(authOptions)
            .ControlEmail(registerDto.Email)
            .ControlPassword(registerDto.Password)
            .Execute();
        if (errors != null)
        {
            return ServiceResultFactory<ProblemsMessage>.Fail(errors);
        }

        if (await userRepository.IsExistAsync(registerDto.Email))
        {
            return ServiceResultFactory.Fail("User already exists");
        }

        var app = await appsRepository.GetAppAsync(registerDto.AppKey);
        if (app == null)
        {
            return ServiceResultFactory.Fail("Provider not found");
        }

        var role = await userRepository.GetRoleAsync(roleName);
        if (role == null)
        {
            return ServiceResultFactory.Fail("Role not found");
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

        return ServiceResultFactory.Ok();
    }

    public async Task<ServiceResult> LoginAsync(LoginDto loginDto)
    {
        var user = await userRepository.GetUserAsync(loginDto.Username);
        if (user == null)
        {
            return ServiceResultFactory.Fail("Credentials error");
        }
        if (user.Active == false)
        {
            return ServiceResultFactory.Fail("User is not active");
        }
        if (!DataHasher.ControlHash(loginDto.Password, user.PasswordHash, user.PasswordSalt))
        {
            return ServiceResultFactory.Fail("Credentials error");
        }

        string? appKey = null;
        if (loginDto.AppKey != null)
        {
            var app = await appsRepository.GetAppAsync(loginDto.AppKey);
            if (app == null)
            {
                return ServiceResultFactory.Fail("Provider not found");
            }

            appKey = app.Key;
        }

        var (accessToken, refreshToken) = await ProduceJwtToken(user, appKey);

        var jwtToken = new JwtToken
        {
            TokenType = TokenTypeList.BearToken,
            Token = accessToken,
            RefreshToken = refreshToken
        };

        return ServiceResultFactory<JwtToken>.Ok(jwtToken);
    }

    public async Task<ServiceResult> RefreshAsync(string? refreshToken)
    {
        if (string.IsNullOrEmpty(refreshToken))
        {
            return ServiceResultFactory.Fail("Invalid token");
        }

        var tokenBuilder = new JwtTokenBuilder(jwtOptions.Issuer, jwtOptions.Audience, jwtOptions.Key);
        var claims = tokenBuilder.ControlToken(refreshToken);

        var username = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
        if (string.IsNullOrEmpty(username))
        {
            return ServiceResultFactory.Fail("Invalid token");
        }

        var user = await userRepository.GetUserAsync(username);
        if (user is null)
        {
            return ServiceResultFactory.Fail("Invalid token");
        }

        var token = await tokenManager.GetLastTokenAsync(user.Id,
            LoginProviders.Internal,
            TokenTypeList.RefreshToken);
        if (token is null)
        {
            return ServiceResultFactory.Fail("Invalid token");
        }

        if (token.Value != refreshToken)
        {
            return ServiceResultFactory.Fail("Invalid token");
        }

        var appKey = claims.FirstOrDefault(c => c.Type == ClaimTypes.Spn)?.Value;
        var (accessToken, newRefreshToken) = await ProduceJwtToken(user, appKey);

        var jwtToken = new JwtToken
        {
            TokenType = TokenTypeList.BearToken,
            Token = accessToken,
            RefreshToken = newRefreshToken
        };

        return ServiceResultFactory<JwtToken>.Ok(jwtToken);
    }

    public async Task<ServiceResult> LogoutAsync(string refreshToken)
    {
        if (string.IsNullOrEmpty(refreshToken))
        {
            return ServiceResultFactory.Fail("Invalid token");
        }

        var tokenBuilder = new JwtTokenBuilder(jwtOptions.Issuer, jwtOptions.Audience, jwtOptions.Key);
        var claims = tokenBuilder.ControlToken(refreshToken);

        var username = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
        if (string.IsNullOrEmpty(username))
        {
            return ServiceResultFactory.Fail("Invalid token");
        }

        var user = await userRepository.GetUserAsync(username);
        if (user is null)
        {
            return ServiceResultFactory.Fail("Invalid token");
        }

        var token = await tokenManager.GetLastTokenAsync(user.Id,
            LoginProviders.Internal,
            TokenTypeList.RefreshToken);
        if (token is null)
        {
            return ServiceResultFactory.Fail("Invalid token");
        }

        if (token.Value != refreshToken)
        {
            return ServiceResultFactory.Fail("Invalid token");
        }

        await tokenManager.RevokeTokenAsync(token);

        return ServiceResultFactory.Ok();
    }

    private async Task<(string jwt, string refresh)> ProduceJwtToken(User user, string? appKey)
    {
        var jwtBuilder = new JwtTokenBuilder(jwtOptions.Issuer, jwtOptions.Audience, jwtOptions.Key)
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