using BlazingFastPublishQueue.Models;

namespace BlazingFastPublishQueue.Server.Models
{
    public class Filter
    {
        public string? Query { get; set; }
        public PublishState? State { get; set; }
        public ItemType? ItemType { get; set; }
        public string? Publication { get; set; }
        public string? PublishTarget { get; set; }
        public string? User { get; set; }
        public string? Server { get; set; }
        public MudBlazor.DateRange? DateRange { get; set; }
        public bool? Published { get; set; }
    }
}
