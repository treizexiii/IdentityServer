namespace Identity.Core.Configuration;

public interface IAuthOptions
{
    int RefreshTokenExpiration { get; }
    int AccessTokenExpiration { get; }
    string PasswordRegex { get; }
    string EmailRegex { get; }
    string AppNameRegex { get; }
}