using BlazingFastPublishQueue.Server.Models;

namespace BlazingFastPublishQueue.Server.ViewModels
{
    public class PublishTransactionViewModel
    {
        public string Id { get; set; }
        public int Total { get; set; }
        public PublishTransaction PublishTransaction { get; set; }

    }
}
