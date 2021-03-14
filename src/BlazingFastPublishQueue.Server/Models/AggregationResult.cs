using System.Collections.Generic;

namespace BlazingFastPublishQueue.Server.Models
{
    public class AggregationResult : Dictionary<string, IEnumerable<AggregationBucket>>
    {

    }    
    
    public struct AggregationBucket
    {
        public string Key { get; init; }
        public long? Count { get; init; }
    }
}
