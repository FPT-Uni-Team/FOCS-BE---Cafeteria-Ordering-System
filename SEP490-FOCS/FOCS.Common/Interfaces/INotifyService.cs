using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FOCS.Common.Interfaces
{
    public interface INotifyService
    {
        Task AddNotifyAsync(string actorId, string message);
        Task<IEnumerable<string>> GetNotifiesAsync(string actorId);
        Task ClearNotifiesAsync(string actorId);
    }
}
