namespace Identity.Wrappers.Dto;

public record AppConfigDto(
    Guid AppId,
    int TokenExpiration,
    int RefreshTokenExpiration,
    string Issuer,
    string Audience,
    DateTime LastUpdated);

public record AppViewDto(
    Guid Id,
    string Name,
    string Description,
    string ApiKey,
    DateTime CreatedAt,
    AppConfigDto Configuration);

public record AppUserDto(
    Guid Id,
    string Email,
    string Username,
    string Role,
    string PhoneNumber,
    bool IsEmailConfirmed,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? DeletedAt);

public record AppRoleDto(
    Guid Id,
    string Name,
    string Description,
    DateTime CreatedAt,
    bool CouldBeDeleted,
    Guid AppId);

public record AppRoleCreateDto(
    string Name,
    string Description);