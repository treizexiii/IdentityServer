using System.Data;
using Identity.Core.Configuration;
using Identity.Core.Entities;
using Identity.Core.Managers;
using Identity.Core.Repositories;
using Identity.Core.Tools;
using Identity.Services.Factories;
using Identity.Wrappers.Dto;

namespace Identity.Services.Admin;

internal class AdminService(IAppsRepository appsRepository, ISecretManager secretManager, ISecurityProvider securityProvider) : IAdminService
{
    private const string Algorithm = Core.Tools.Algorithm.HmacSha256;

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
                Description = registerAppDto.Description,
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
            return ServiceResultFactory<AppDto>.Fail(e.Message, null);
        }
    }
}