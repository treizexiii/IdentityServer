using Microsoft.Extensions.Configuration;

namespace Identity.Services.Auth;

public interface IAuthOptions
{
    string Issuer { get; }
    string Audience { get; }
    string Key { get; }
}

public class AuthOptions : IAuthOptions
{
    private readonly IConfiguration _configuration;

    public AuthOptions(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string Issuer => _configuration["Jwt:Issuer"];

    public string Audience => _configuration["Jwt:Audience"];
    public string Key => _configuration["Jwt:Key"];
}