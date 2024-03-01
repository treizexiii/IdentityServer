using System.Data;
using Identity.Core.Configuration;
using Identity.Core.Entities;
using Identity.Core.Managers;
using Identity.Core.Repositories;
using Identity.Core.Tools;
using Identity.Services.Factories;
using Identity.Wrappers.Dto;

namespace Identity.Services.Admin;

internal class AdminService(
    IAppsRepository appsRepository,
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
            return ServiceResultFactory<IEnumerable<AppViewDto>>.Fail(e.Message, 400, null);
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
            return ServiceResultFactory<AppViewDto>.Fail(e.Message, 400, null);
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
                throw new DataException("App already exists");
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
            return ServiceResultFactory<AppDto>.Fail(e.Message, 400, null);
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
            return ServiceResultFactory<AppDto>.Fail(e.Message, 400, null);
        }
    }

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
            return ServiceResultFactory<IEnumerable<AppUserDto>>.Fail(e.Message, 400, null);
        }
    }
}