using Microsoft.Extensions.Caching.Distributed;

namespace BeatIt.Services.CacheService;
public class CacheService(IDistributedCache cache, ILogger<CacheService> logger) : ICacheService
{
    private readonly IDistributedCache _cache = cache;
    private readonly ILogger<CacheService> _logger = logger;
    public async Task<string> GetFromCache(string key)
    {
        var value = await _cache.GetStringAsync(key);
        if (value == null)
        {
            _logger.LogDebug("Can't find key value from cache");
        }
        _logger.LogDebug("Cache returned value with success");
        return value!;
    }

    public async Task StoreOnCache(string key, string value, TimeSpan expiration)
    {
        await _cache.SetStringAsync(key, value, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration
        });
    }
}