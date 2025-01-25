namespace BeatIt.Services.CacheService;

public interface ICacheService
{
    Task<string> GetAsync(string key);
    Task StoreOnCache(string key, string value, TimeSpan expiration);
    Task RemoveFromCache(string key);
}