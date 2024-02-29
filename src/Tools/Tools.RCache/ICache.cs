namespace Tools.RCache;

public interface ICache
{
    Task<T?> GetAsync<T>(string key);
    Task SetAsync<T>(string key, T value, TimeSpan? expiry = null);
    Task RemoveAsync<T>(string key);
    Task<bool> ExistsAsync<T>(string key);
    Task RefreshAsync<T>(string key);
}