using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;

namespace Tools.RCache;

public class RedisCache : ICache
{
    private readonly IDistributedCache _cache;
    private readonly IConnectionMultiplexer _connection;

    public RedisCache(IDistributedCache cache, IConnectionMultiplexer connection)
    {
        _cache = cache;
        _connection = connection;
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        var formattedKey = FormatKey<T>(key);
        var value = await _cache.GetStringAsync(formattedKey);
        return value == null ?
            default :
            JsonSerializer.Deserialize<T>(value);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
    {
        var formattedKey = FormatKey<T>(key);
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiry
        };
        var serializedValue = JsonSerializer.Serialize(value);
        await _cache.SetStringAsync(formattedKey, serializedValue, options);
    }

    public async Task RemoveAsync<T>(string key)
    {
        var formattedKey = FormatKey<T>(key);
        await _cache.RemoveAsync(formattedKey);
    }

    public async Task<bool> ExistsAsync<T>(string key)
    {
        var formattedKey = FormatKey<T>(key);
        var value = await _cache.GetStringAsync(formattedKey);
        return value != null;
    }

    public async Task RefreshAsync<T>(string key)
    {
        var formattedKey = FormatKey<T>(key);
        await _cache.RefreshAsync(formattedKey);
    }

    private static string FormatKey<T>(string key)
    {
        var type = typeof(T);
        var formattedKey = type.Name + ":" + key;
        return formattedKey;
    }
}