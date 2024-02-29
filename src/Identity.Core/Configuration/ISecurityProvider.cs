namespace Identity.Core.Configuration;

public interface ISecurityProvider
{
    string GetSecretHashingSalt();
}