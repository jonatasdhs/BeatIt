using System.Text;
using Microsoft.Extensions.Caching.Distributed;

namespace BeatIt.Services.CacheService;
public class CacheService(IDistributedCache cache, ILogger<CacheService> logger) : ICacheService
{
    private readonly IDistributedCache _cache = cache;
    private readonly ILogger<CacheService> _logger = logger;
    public async Task<string> GetAsync(string key)
    {
        var valueBytes = await _cache.GetAsync(key);
        if (valueBytes == null)
        {
            _logger.LogWarning("Can't find key value from cache");
            return null!;
        }
        var value = Encoding.UTF8.GetString(valueBytes);
        _logger.LogDebug("Cache returned value with success");
        return value;
    }

    public async Task StoreOnCache(string key, string value, TimeSpan expiration)
    {
        var valueBytes = Encoding.UTF8.GetBytes(value);
        await _cache.SetAsync(key, valueBytes, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration
        });
        _logger.LogDebug("Cache stored value with success");
    }
}