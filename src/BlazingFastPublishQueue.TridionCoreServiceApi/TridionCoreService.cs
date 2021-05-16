using BlazingFastPublishQueue.Models.Contracts;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BlazingFastPublishQueue.TridionCoreServiceApi
{
    public class TridionCoreService : ITridionCoreService
    {
        private readonly ILogger _logger;

        public TridionCoreService(ILogger<TridionCoreService> logger)
        {
            _logger = logger;
        }

        public Task<bool> RePublish(IEnumerable<string> ids)
        {
            _logger.LogInformation("Republishing ids");
            return Task.FromResult(false);
        }
    }
}
