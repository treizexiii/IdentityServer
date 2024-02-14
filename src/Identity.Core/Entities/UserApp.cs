namespace Identity.Core.Entities;

public class UserApp
{
    public Guid UserId { get; set; }
    public Guid AppId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public virtual App App { get; set; } = null!;
}