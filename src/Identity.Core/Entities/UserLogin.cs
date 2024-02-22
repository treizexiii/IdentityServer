namespace Identity.Core.Entities;

public class UserLogin
{
    public Guid UserId { get; set; }
    public string LoginProvider { get; set; } = string.Empty;
    public string ProviderKey { get; set; } = string.Empty;
    public string ProviderDisplayName { get; set; } = string.Empty;
    public virtual User User { get; set; } = null!;
    public DateTimeOffset CreatedAt { get; set; }
}