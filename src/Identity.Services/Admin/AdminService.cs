using System.Data;
using Identity.Core.Entities;
using Identity.Core.Repositories;
using Identity.Wrappers.Dto;

namespace Identity.Services.Admin;

public interface IAdminService
{
    Task<string> CreateAppAsync(RegisterAppDto registerAppDto);
}

public class AdminService(IAppsRepository appsRepository) : IAdminService
{
    public async Task<string> CreateAppAsync(RegisterAppDto registerAppDto)
    {
        var normalizeName = registerAppDto.AppName.Normalize();
        var app = new App();
        app.Id = Guid.NewGuid();
        app.Key = app.Id.ToString().Normalize();
        app.Name = registerAppDto.AppName;
        app.NormalizedName = normalizeName;

        if (await appsRepository.IsExistAsync(app.NormalizedName)) throw new DataException("App already exists");

        await appsRepository.AddAppAsync(app);

        return app.Key;
    }
}