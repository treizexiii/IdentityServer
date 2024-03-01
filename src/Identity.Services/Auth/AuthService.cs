using System.Security.Claims;
using System.Text;
using Identity.Core.Configuration;
using Identity.Core.Entities;
using Identity.Core.Managers;
using Identity.Core.Repositories;
using Identity.Core.Tools;
using Identity.Services.Factories;
using Identity.Wrappers.Dto;

namespace Identity.Services.Auth;

internal class AuthService(
    IUsersRepository userRepository,
    ITokenManager tokenManager,
    ISecretManager secretManager,
    ISecurityProvider securityProvider,
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
        if (errors != null) return ServiceResultFactory.Fail(errors);

        if (await userRepository.IsExistAsync(registerDto.Email))
            return ServiceResultFactory.Fail("User already exists");

        var app = await appsRepository.GetAppAsync(registerDto.AppKey);
        if (app == null)
            return ServiceResultFactory.Fail("Provider not found");

        var role = await userRepository.GetRoleAsync(roleName);
        if (role == null)
            return ServiceResultFactory.Fail("Role not found");

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

        user.UserRoles.Add(new UserRole
        {
            UserId = user.Id,
            RoleId = role.Id,
            CreatedAt = utc,
            AppId = app.Id
        });

        if (role.Name == RolesList.Admin)
        {
            app.Owner = user.Id;
            await appsRepository.UpdateAppAsync(app);
        }

        await userRepository.AddUserAsync(user);

        return ServiceResultFactory.Ok();
    }

    public async Task<ServiceResult<JwtToken>> LoginAsync(LoginDto loginDto, string apiKey)
    {
        var user = await userRepository.GetUserAsync(loginDto.Username);
        if (user == null)
        {
            return ServiceResultFactory<JwtToken>.Fail("Credentials error");
        }

        if (user.Active == false)
        {
            return ServiceResultFactory<JwtToken>.Fail("User is not active");
        }

        if (!DataHasher.ControlHash(loginDto.Password, user.PasswordHash, user.PasswordSalt))
        {
            return ServiceResultFactory<JwtToken>.Fail("Credentials error");
        }

        var app = await appsRepository.GetAppAsync(apiKey);
        if (app == null)
        {
            return ServiceResultFactory<JwtToken>.Fail("client not found");
        }

        var userRole = user.UserRoles.FirstOrDefault(ur => ur.AppId == app.Id);
        if (userRole == null)
        {
            return ServiceResultFactory<JwtToken>.Fail("client not found");
        }

        var secret = await secretManager.GetSecretAsync(app.Id, SecretTypes.AppKey);
        if (secret == null)
        {
            return ServiceResultFactory<JwtToken>.Fail("client not found");
        }

        var (accessToken, refreshToken) =
            await ProduceJwtToken(
                user,
                userRole,
                DataHasher.Decrypt(secret.Value, Encoding.UTF8.GetBytes(securityProvider.GetSecretHashingSalt())));

        var jwtToken = new JwtToken
        {
            TokenType = TokenTypeList.BearToken,
            Token = accessToken,
            RefreshToken = refreshToken
        };

        return ServiceResultFactory<JwtToken>.Ok(jwtToken);
    }

    public async Task<ServiceResult<JwtToken>> RefreshAsync(string? refreshToken, string apiKey)
    {
        if (string.IsNullOrEmpty(refreshToken))
        {
            return ServiceResultFactory<JwtToken>.Fail("Invalid token");
        }

        var app = await appsRepository.GetAppAsync(apiKey);
        if (app is null)
        {
            return ServiceResultFactory<JwtToken>.Fail("Invalid client");
        }

        var secret = await secretManager.GetSecretAsync(app.Id, SecretTypes.AppKey);
        if (secret is null)
        {
            return ServiceResultFactory<JwtToken>.Fail("Invalid client");
        }

        var key = DataHasher.Decrypt(secret.Value, Encoding.UTF8.GetBytes(securityProvider.GetSecretHashingSalt())).Split(":");

        var tokenBuilder = new JwtTokenBuilder(
            jwtOptions.Issuer,
            jwtOptions.Audience,
            key[1],
            key[0]);
        var claims = tokenBuilder.ControlToken(refreshToken);

        var username = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
        if (string.IsNullOrEmpty(username))
        {
            return ServiceResultFactory<JwtToken>.Fail("Invalid token");
        }

        var user = await userRepository.GetUserAsync(username);
        if (user is null)
        {
            return ServiceResultFactory<JwtToken>.Fail("Invalid token");
        }

        var token = await tokenManager.GetLastTokenAsync(user.Id,
            LoginProviders.Internal,
            TokenTypeList.RefreshToken);
        if (token is null)
        {
            return ServiceResultFactory<JwtToken>.Fail("Invalid token");
        }

        if (token.Value != refreshToken)
        {
            return ServiceResultFactory<JwtToken>.Fail("Invalid token");
        }

        var userRole = user.UserRoles.FirstOrDefault(ur => ur.AppId == app.Id);
        if (userRole is null)
        {
            return ServiceResultFactory<JwtToken>.Fail("Invalid client");
        }
        var (accessToken, newRefreshToken) = await ProduceJwtToken(user, userRole, key[1], key[0]);

        var jwtToken = new JwtToken
        {
            TokenType = TokenTypeList.BearToken,
            Token = accessToken,
            RefreshToken = newRefreshToken
        };

        return ServiceResultFactory<JwtToken>.Ok(jwtToken);
    }

    public async Task<ServiceResult> LogoutAsync(string refreshToken, string apiKey)
    {
        if (string.IsNullOrEmpty(refreshToken))
        {
            return ServiceResultFactory.Fail("Invalid token");
        }

        var app = await appsRepository.GetAppAsync(apiKey);
        if (app is null)
        {
            return ServiceResultFactory<JwtToken>.Fail("Invalid client");
        }

        var secret = await secretManager.GetSecretAsync(app.Id, SecretTypes.AppKey);
        if (secret is null)
        {
            return ServiceResultFactory<JwtToken>.Fail("Invalid client");
        }

        var key = DataHasher.Decrypt(
            secret.Value,
            Encoding.UTF8.GetBytes(securityProvider.GetSecretHashingSalt())).Split(":");

        var tokenBuilder = new JwtTokenBuilder(jwtOptions.Issuer, jwtOptions.Audience, key[1], key[0]);
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

    private async Task<(string jwt, string refresh)> ProduceJwtToken(User user, UserRole role, string? secret)
    {
        var algo = secret.Split(":")[0];
        var key = secret.Split(":")[1];
        return await ProduceJwtToken(user, role, key, algo);
    }

    private async Task<(string jwt, string refresh)> ProduceJwtToken(User user, UserRole role, string key, string algo)
    {
        var jwtBuilder = new JwtTokenBuilder(jwtOptions.Issuer, jwtOptions.Audience, key, algo)
            .AddUser(user)
            .AddRole(role.Role);

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