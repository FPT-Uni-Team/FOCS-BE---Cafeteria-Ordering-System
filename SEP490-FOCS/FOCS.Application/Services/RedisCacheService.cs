using FOCS.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace FOCS.Application.Services
{
    public class RedisCacheService : IRedisCacheService
    {
        private readonly IDatabase _database;

        public RedisCacheService(IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("Redis")
                            ?? configuration["Redis:ConnectionString"];

            var redis = ConnectionMultiplexer.Connect(connectionString);
            _database = redis.GetDatabase();
        }

        public async Task<List<string>> GetKeysByPatternAsync(string pattern)
        {
            var endpoints = _database.Multiplexer.GetEndPoints();
            var server = _database.Multiplexer.GetServer(endpoints.First());

            var keys = server.Keys(pattern: pattern).ToList();
            return keys.Select(k => k.ToString()).ToList();
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
