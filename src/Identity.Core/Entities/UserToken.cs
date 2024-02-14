
namespace Identity.Core.Entities;

public class UserToken
{
    public Guid UserId { get; set; }
    public virtual User User { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string LoginProvider { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
}

public static class LoginProviders
{
    public const string Internal = "Internal";
    public const string Google = "Google";
    public const string Facebook = "Facebook";
    public const string Twitter = "Twitter";
    public const string Microsoft = "Microsoft";
    public const string GitHub = "GitHub";
}

public static class TokenTypeList
{
    public const string BearToken = "Bearer";
    public const string RefreshToken = "RefreshToken";
}