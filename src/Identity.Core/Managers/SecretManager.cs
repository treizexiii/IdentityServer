using Identity.Core.Entities;
using Identity.Core.Repositories;

namespace Identity.Core.Managers;

public interface ISecretManager
{
    Task<Secret?> GetSecretAsync(Guid objectId, string secretType);
    Task<Secret> GenerateSecretAsync(Guid objectId, string secretType, byte[] value, byte[]? salt = null);
    Task<int> RevokeSecretAsync(Guid objectId, string secretType);
}

public class SecretManager(ISecretStore<Secret> secretStore) : ISecretManager
{
    public async Task<Secret?> GetSecretAsync(Guid objectId, string secretType)
    {
        var query = secretStore.Where(s => s.ObjectId == objectId && s.SecretType == secretType && s.DeletedAt == null);
        return await secretStore.GetTokenAsync(query);
    }

    public async Task<Secret> GenerateSecretAsync(Guid objectId, string secretType, byte[] value, byte[]? salt = null)
    {
        var oldSecret = await GetSecretAsync(objectId, secretType);
        if (oldSecret is not null)
        {
            oldSecret.DeletedAt = DateTimeOffset.UtcNow;
            await secretStore.UpdateAsync(oldSecret);
        }

        var secret = new Secret
        {
            ObjectId = objectId,
            SecretType = secretType,
            Value = value,
            CreatedAt = DateTimeOffset.UtcNow
        };

        if (salt is not null)
        {
            secret.Salt = salt;
        }

        await secretStore.AddAsync(secret);
        return secret;
    }

    public async Task<int> RevokeSecretAsync(Guid objectId, string secretType)
    {
        var secret = await GetSecretAsync(objectId, secretType);
        if (secret is null)
        {
            return 1;
        }
        secret.DeletedAt = DateTimeOffset.UtcNow;
        await secretStore.UpdateAsync(secret);
        return 0;
    }
}