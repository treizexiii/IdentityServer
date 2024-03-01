namespace Identity.Wrappers.Dto;

public record AppConfigDto(Guid AppId, int TokenExpiration, int RefreshTokenExpiration, string Issuer, string Audience, DateTime LastUpdated);

public record AppViewDto(Guid Id, string Name, string Description, string ApiKey, DateTime CreatedAt, AppConfigDto Configuration);