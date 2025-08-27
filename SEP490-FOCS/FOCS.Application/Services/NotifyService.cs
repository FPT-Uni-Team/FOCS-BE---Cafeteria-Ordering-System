using FOCS.Common.Interfaces;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace FOCS.Application.Services
{
    public class NotifyService: INotifyService
    {
        private readonly IDatabase _redis;

        public NotifyService(IConnectionMultiplexer connectionMultiplexer)
        {
            _redis = connectionMultiplexer.GetDatabase();
        }

        private string GetKey(string actorId) => $"notify:{actorId}";

        public async Task AddNotifyAsync(string actorId, string message)
        {
            var notify = new
            {
                Id = Guid.NewGuid().ToString(),
                Message = message,
                CreatedAt = DateTime.UtcNow
            };

            string key = GetKey(actorId);
            string value = JsonSerializer.Serialize(notify);

            await _redis.ListLeftPushAsync(key, value);

            await _redis.ListTrimAsync(key, 0, 49);
        }

        public async Task<IEnumerable<string>> GetNotifiesAsync(string actorId)
        {
            string key = GetKey(actorId);

            var values = await _redis.ListRangeAsync(key, 0, -1);
            return values.Select(x => x.ToString());
        }

        public async Task ClearNotifiesAsync(string actorId)
        {
            string key = GetKey(actorId);
            await _redis.KeyDeleteAsync(key);
        }
    }
}
