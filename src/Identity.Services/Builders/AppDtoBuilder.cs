using Identity.Core.Entities;
using Identity.Wrappers.Dto;

namespace Identity.Services.Builders;

public class AppDtoBuilder(App app)
{
    public AppViewDto Build()
    {
        var config = new AppConfigDto(
            app.Id,
            app.Configuration.TokenExpiration,
            app.Configuration.RefreshTokenExpiration,
            app.Configuration.Issuer,
            app.Configuration.Audience,
            app.Configuration.UpdatedAt.DateTime);

        return new AppViewDto(app.Id, app.Name, app.Description, app.ApiKey, app.CreatedAt.DateTime, config);
    }
}
