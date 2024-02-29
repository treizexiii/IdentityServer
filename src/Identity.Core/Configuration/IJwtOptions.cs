namespace Identity.Core.Configuration;

public interface IJwtOptions
{
    string Issuer { get; }
    string Audience { get; }
    string Key { get; }
}