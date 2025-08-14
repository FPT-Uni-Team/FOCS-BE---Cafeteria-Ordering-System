using FOCS.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace FOCS.Application.Services
{
    public class RedisCacheService : IRedisCacheService
    {
        private readonly IDatabase _database;
        private readonly IConnectionMultiplexer _connection;

        private readonly ILogger<RedisCacheService> _loggerRedis;
        public RedisCacheService(IConfiguration configuration, ILogger<RedisCacheService> loggerRedis)
        {
            var connectionString = configuration.GetConnectionString("Redis")
                                ?? configuration["Redis:ConnectionString"];
            _loggerRedis = loggerRedis;

            _connection = ConnectionMultiplexer.Connect(connectionString);
            _database = _connection.GetDatabase();
        }

        public async Task<List<string>> GetKeysByPatternAsync(string pattern)
        {
            var endpoints = _connection.GetEndPoints();
            var server = _connection.GetServer(endpoints.First());

            var keys = server.Keys(pattern: pattern).ToList();
            return keys.Select(k => k.ToString()).ToList();
        }

        public async Task<IEnumerable<RedisKey>> GetAllKeysAsync(string pattern = "*")
        {
            var server = _connection.GetServer(_connection.GetEndPoints().First());
            return server.Keys(pattern: pattern);
        }

        public async Task<Dictionary<string, T>> GetAllAsync<T>(string pattern = "*")
        {
            var server = _connection.GetServer(_connection.GetEndPoints().First());
            var result = new Dictionary<string, T>();
            const int batchSize = 1000;

            var keys = server.Keys(pattern: pattern, pageSize: batchSize);

            var batchKeys = new List<RedisKey>(batchSize);
            foreach (var key in keys)
            {
                batchKeys.Add(key);
                if (batchKeys.Count >= batchSize)
                {
                    var values = await _database.StringGetAsync(batchKeys.ToArray());
                    for (int i = 0; i < batchKeys.Count; i++)
                    {
                        if (values[i].HasValue)
                        {
                            var obj = JsonSerializer.Deserialize<T>(values[i]);
                            result[batchKeys[i]] = obj;
                        } else
                        {
                            _loggerRedis.LogInformation($"The key: {key} is empty value");
                        }
                    }
                    batchKeys.Clear();
                }
            }
            // Process remaining keys
            if (batchKeys.Count > 0)
            {
                var values = await _database.StringGetAsync(batchKeys.ToArray());
                for (int i = 0; i < batchKeys.Count; i++)
                {
                    if (values[i].HasValue)
                    {
                        var obj = JsonSerializer.Deserialize<T>(values[i]);
                        result[batchKeys[i]] = obj;
                    }
                }
            }

            return result;
        }


        public async Task<bool> ExistsAsync(string key)
        {
            return await _database.KeyExistsAsync(key);
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            var value = await _database.StringGetAsync(key);
            if (value.IsNullOrEmpty)
                return default;

            return JsonSerializer.Deserialize<T>(value!);
        }

        public async Task<bool> RemoveAsync(string key)
        {
            return await _database.KeyDeleteAsync(key);
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
        {
            var json = JsonSerializer.Serialize(value);
            await _database.StringSetAsync(key, json, expiry);
        }
    }
}
