using Microsoft.Extensions.Configuration;

namespace Identity.Services.Auth;

public interface IJwtOptions
{
    string Issuer { get; }
    string Audience { get; }
    string Key { get; }
}

public class JwtOptions(IConfiguration configuration) : IJwtOptions
{
    public string Issuer => configuration["Jwt:Issuer"];

    public string Audience => configuration["Jwt:Audience"];
    public string Key => configuration["Jwt:Key"];
}
