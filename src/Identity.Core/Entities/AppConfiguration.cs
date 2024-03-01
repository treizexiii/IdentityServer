namespace Identity.Core.Entities;

public class AppConfiguration
{
    public Guid Id { get; set; }
    public Guid AppId { get; set; }
    public int TokenExpiration { get; set; }
    public int RefreshTokenExpiration { get; set; }
    public string Issuer { get; set; }
    public string Audience { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}