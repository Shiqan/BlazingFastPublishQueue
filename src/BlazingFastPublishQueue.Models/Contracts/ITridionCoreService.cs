using System.Collections.Generic;
using System.Threading.Tasks;

namespace BlazingFastPublishQueue.Models.Contracts
{
    public interface ITridionCoreService
    {
        Task<bool> RePublish(IEnumerable<string> ids);
    }
}
