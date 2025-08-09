using FOCS.Common.Interfaces;
using Microsoft.Extensions.Configuration;
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

        public RedisCacheService(IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("Redis")
                                ?? configuration["Redis:ConnectionString"];

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
            var keys = server.Keys(pattern: pattern);
            var result = new Dictionary<string, T>();

            foreach (var key in keys)
            {
                var value = await _database.StringGetAsync(key);
                result[key] = JsonSerializer.Deserialize<T>(value);
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
