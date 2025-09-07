using Microsoft.Extensions.Caching.Memory;

namespace TunCRM.Services
{
    public class CacheService
    {
        private readonly IMemoryCache _cache;
        private readonly ILogger<CacheService> _logger;
        private readonly TimeSpan _defaultCacheDuration = TimeSpan.FromMinutes(15);

        public CacheService(IMemoryCache cache, ILogger<CacheService> logger)
        {
            _cache = cache;
            _logger = logger;
        }

        public async Task<T?> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? duration = null)
        {
            if (_cache.TryGetValue(key, out T? cachedValue))
            {
                _logger.LogDebug($"Cache hit for key: {key}");
                return cachedValue;
            }

            _logger.LogDebug($"Cache miss for key: {key}");
            var value = await factory();
            
            if (value != null)
            {
                var cacheOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = duration ?? _defaultCacheDuration,
                    SlidingExpiration = TimeSpan.FromMinutes(5),
                    Priority = CacheItemPriority.Normal
                };

                _cache.Set(key, value, cacheOptions);
                _logger.LogDebug($"Cached value for key: {key}");
            }

            return value;
        }

        public T? Get<T>(string key)
        {
            _cache.TryGetValue(key, out T? value);
            return value;
        }

        public void Set<T>(string key, T value, TimeSpan? duration = null)
        {
            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = duration ?? _defaultCacheDuration,
                SlidingExpiration = TimeSpan.FromMinutes(5),
                Priority = CacheItemPriority.Normal
            };

            _cache.Set(key, value, cacheOptions);
            _logger.LogDebug($"Set cache for key: {key}");
        }

        public void Remove(string key)
        {
            _cache.Remove(key);
            _logger.LogDebug($"Removed cache for key: {key}");
        }

        public void RemoveByPattern(string pattern)
        {
            // MemoryCache doesn't support pattern removal directly
            // This is a simplified implementation
            _logger.LogDebug($"Pattern removal requested for: {pattern}");
        }

        public void Clear()
        {
            if (_cache is MemoryCache memoryCache)
            {
                memoryCache.Clear();
                _logger.LogInformation("Cache cleared");
            }
        }

        public long GetCacheSize()
        {
            // MemoryCache doesn't expose size directly
            // This is a placeholder implementation
            return 0;
        }
    }
}
