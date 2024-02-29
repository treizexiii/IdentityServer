using Identity.Core.Configuration;
using Microsoft.Extensions.Configuration;

namespace Identity.Services.Configuration;

internal class SecurityProvider(IConfiguration configuration) : ISecurityProvider
{
    public string GetSecretHashingSalt() => configuration["Security:SecretHashingSalt"];
}