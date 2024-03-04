using System.Data;
using Identity.Core.Configuration;
using Identity.Core.Entities;
using Identity.Core.Managers;
using Identity.Core.Repositories;
using Identity.Core.Tools;
using Identity.Services.Builders;
using Identity.Services.Factories;
using Identity.Wrappers.Dto;

namespace Identity.Services.Admin;

internal class AdminService(
    IAppsRepository appsRepository,
    IRolesRepository rolesRepository,
    ISecretManager secretManager,
    ISecurityProvider securityProvider) : IAdminService
{
    private const string Algorithm = Core.Tools.Algorithm.HmacSha256;

    public async Task<ServiceResult<IEnumerable<AppViewDto>>> GetAppsAsync(Guid ownerId)
    {
        try
        {
            var apps = await appsRepository.GetAppsAsync(ownerId);
            var list = new List<AppViewDto>();
            foreach (var app in apps)
            {
                var dto = new AppDtoBuilder(app).Build();
                list.Add(dto);
            }

            return ServiceResultFactory<IEnumerable<AppViewDto>>.Ok(list);
        }
        catch (Exception e)
        {
            return ServiceResultFactory<IEnumerable<AppViewDto>>.Fail(e.Message, 500, null);
        }
    }

    public async Task<ServiceResult<AppViewDto>> GetAppAsync(Guid owner, Guid appId)
    {
        try
        {
            var app = await appsRepository.GetAppAsync(appId);
            if (app is null)
            {
                return ServiceResultFactory<AppViewDto>.NotFound();
            }

            if (app.Owner != owner)
            {
                return ServiceResultFactory<AppViewDto>.Forbid();
            }

            var dto = new AppDtoBuilder(app).Build();
            return ServiceResultFactory<AppViewDto>.Ok(dto);
        }
        catch (Exception e)
        {
            return ServiceResultFactory<AppViewDto>.Fail(e.Message, 500, null);
        }
    }

    public async Task<ServiceResult<AppDto>> CreateAppAsync(RegisterAppDto registerAppDto)
    {
        try
        {
            var utc = DateTime.UtcNow;

            var normalizeName = registerAppDto.AppName.Normalize();
            if (await appsRepository.IsExistAsync(normalizeName))
            {
                return ServiceResultFactory<AppDto>.Conflict("App with this name already exists.");
            }

            var app = new App
            {
                Id = Guid.NewGuid(),
                ApiKey = Guid.NewGuid().ToString(),
                Name = registerAppDto.AppName,
                NormalizedName = normalizeName,
                Description = registerAppDto.Description ?? string.Empty,
                CreatedAt = utc
            };

            var rawSecret = Algorithm + ":" + Guid.NewGuid();
            var hashedSecret = DataHasher.Hash(rawSecret, securityProvider.GetSecretHashingSalt());
            await secretManager.GenerateSecretAsync(app.Id, SecretTypes.AppKey, hashedSecret.hash);

            await appsRepository.AddAppAsync(app);

            var registeredApp = new AppDto(app.Id, app.Name, app.ApiKey, rawSecret);

            return ServiceResultFactory<AppDto>.Ok(registeredApp);
        }
        catch (Exception e)
        {
            return ServiceResultFactory<AppDto>.Fail(e.Message, 500, null);
        }
    }

    public async Task<ServiceResult> UpdateAppAsync(Guid owner, Guid appId, AppConfigDto configuration)
    {
        try
        {
            var app = await appsRepository.GetAppAsync(appId);
            if (app is null)
            {
                return ServiceResultFactory<AppDto>.NotFound();
            }

            if (app.Owner != owner)
            {
                return ServiceResultFactory<AppDto>.Forbid();
            }

            var utc = DateTime.UtcNow;

            app.Configuration.TokenExpiration = configuration.TokenExpiration;
            app.Configuration.RefreshTokenExpiration = configuration.RefreshTokenExpiration;
            app.Configuration.Issuer = configuration.Issuer;
            app.Configuration.Audience = configuration.Audience;
            app.Configuration.UpdatedAt = utc;

            await appsRepository.UpdateAppAsync(app);

            return ServiceResultFactory.Ok();
        }
        catch (Exception e)
        {
            return ServiceResultFactory<AppDto>.Fail(e.Message, 500, null);
        }
    }

    #region Role

    public async Task<ServiceResult<IEnumerable<AppRoleDto>>> GetRolesAsync(Guid owner, Guid appId)
    {
        var app = await appsRepository.GetAppAsync(appId);
        if (app is null)
        {
            return ServiceResultFactory<IEnumerable<AppRoleDto>>.NotFound();
        }

        if (app.Owner != owner)
        {
            return ServiceResultFactory<IEnumerable<AppRoleDto>>.Forbid();
        }

        var roles = await rolesRepository.GetRolesAsync(appId);
        var list = new List<AppRoleDto>();
        foreach (var role in roles)
        {
            var dto = new AppRoleDto(
                role.Id,
                role.Name,
                role.Description,
                role.CreatedAt.DateTime,
                role.CouldBeDeleted,
                role.AppId);
            list.Add(dto);
        }

        return ServiceResultFactory<IEnumerable<AppRoleDto>>.Ok(list);
    }

    public async Task<ServiceResult> CreateRoleAsync(Guid userId, Guid appId, AppRoleCreateDto role)
    {
        var app = await appsRepository.GetAppAsync(appId);
        if (app is null)
        {
            return ServiceResultFactory.NotFound();
        }
        if (app.Owner != userId)
        {
            return ServiceResultFactory.Forbid();
        }

        var utc = DateTime.UtcNow;
        var newRole = new Role
        {
            Id = Guid.NewGuid(),
            Name = role.Name,
            Description = role.Description,
            CreatedAt = utc,
            AppId = appId,
            IsSystemDefault = false,
            CouldBeDeleted = true,
        };
        newRole.NormalizedName = newRole.Name.Normalize();
        newRole.ConcurrencyStamp = newRole.Id.ToString();

        await rolesRepository.AddRoleAsync(newRole);

        return ServiceResultFactory.Ok("Role created successfully.");
    }

    public async Task<ServiceResult> DeleteRoleAsync(Guid owner, Guid appId, Guid roleId)
    {
        var app = await appsRepository.GetAppAsync(appId);
        if (app is null)
        {
            return ServiceResultFactory.NotFound();
        }
        if (app.Owner != owner)
        {
            return ServiceResultFactory.Forbid();
        }

        await rolesRepository.DeleteRoleAsync(roleId);

        return ServiceResultFactory.Ok();
    }

    #endregion

    public async Task<ServiceResult<IEnumerable<AppUserDto>>> GetAppUsersAsync(Guid owner, Guid id, string role)
    {
        try
        {
            var app = await appsRepository.GetAppAsync(id);
            if (app is null)
            {
                return ServiceResultFactory<IEnumerable<AppUserDto>>.NotFound();
            }

            if (app.Owner != owner)
            {
                return ServiceResultFactory<IEnumerable<AppUserDto>>.Forbid();
            }

            var users = await appsRepository.GetAppUsersAsync(id, role);
            var list = new List<AppUserDto>();

            foreach (var user in users)
            {
                var dto = new AppUserDtoBuilder(user).Build();
                list.Add(dto);
            }

            return ServiceResultFactory<IEnumerable<AppUserDto>>.Ok(list);
        }
        catch (Exception e)
        {
            return ServiceResultFactory<IEnumerable<AppUserDto>>.Fail(e.Message, 500, null);
        }
    }
}