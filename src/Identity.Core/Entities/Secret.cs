namespace Identity.Core.Entities;

public class Secret
{
    public Guid Id { get; set; }
    public Guid ObjectId { get; set; }
    public string SecretType { get; set; } = string.Empty;
    public DateTimeOffset? DeletedAt { get; set; }
    public byte[] Value { get; set; } = Array.Empty<byte>();
    public byte[]? Salt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

public static class SecretTypes
{
    public const string Password = "Password";
    public const string AppKey = "App";
}
