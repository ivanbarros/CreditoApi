using StackExchange.Redis;
using System.Text.Json;

namespace CreditoAPI.Infrastructure.Cache
{
    public class RedisCacheService : ICacheService
    {
        private readonly IConnectionMultiplexer? _redis;
        private readonly IDatabase? _database;
        private readonly ILogger<RedisCacheService> _logger;

        public RedisCacheService(IConnectionMultiplexer? redis, ILogger<RedisCacheService> logger)
        {
            _redis = redis;
            _database = redis?.GetDatabase();
            _logger = logger;
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            if (_database == null)
            {
                _logger.LogWarning("Redis database not available. Returning default value for key: {Key}", key);
                return default;
            }

            try
            {
                var value = await _database.StringGetAsync(key);
                
                if (value.IsNullOrEmpty)
                    return default;

                return JsonSerializer.Deserialize<T>(value!);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cache key: {Key}", key);
                return default;
            }
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
        {
            if (_database == null)
            {
                _logger.LogWarning("Redis database not available. Cannot set key: {Key}", key);
                return;
            }

            try
            {
                var serialized = JsonSerializer.Serialize(value);
                await _database.StringSetAsync(key, serialized, expiration);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting cache key: {Key}", key);
            }
        }

        public async Task RemoveAsync(string key)
        {
            if (_database == null)
            {
                _logger.LogWarning("Redis database not available. Cannot remove key: {Key}", key);
                return;
            }

            try
            {
                await _database.KeyDeleteAsync(key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing cache key: {Key}", key);
            }
        }

        public async Task<bool> ExistsAsync(string key)
        {
            if (_database == null)
            {
                _logger.LogWarning("Redis database not available. Returning false for key existence: {Key}", key);
                return false;
            }

            try
            {
                return await _database.KeyExistsAsync(key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking cache key existence: {Key}", key);
                return false;
            }
        }
    }
}
