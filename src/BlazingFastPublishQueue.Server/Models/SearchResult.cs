using BlazingFastPublishQueue.Models;
using System.Collections.Generic;

namespace BlazingFastPublishQueue.Server.Models
{
    public class SearchResult
    {
        public IEnumerable<PublishTransaction> Items { get; init; }
        public int TotalItems { get; init; }

    }
}
