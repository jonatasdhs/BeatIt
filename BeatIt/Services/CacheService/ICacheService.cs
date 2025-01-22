namespace BeatIt.Services.CacheService;

public interface ICacheService
{
    Task<string> GetFromCache(string key);
    Task StoreOnCache(string key, string value, TimeSpan expiration);
}