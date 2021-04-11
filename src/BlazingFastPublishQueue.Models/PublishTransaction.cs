using System;
using System.Collections.Generic;

namespace BlazingFastPublishQueue.Models
{
    public class PublishTransaction
    {
        const string TransactionDateFormat = "yyyy-MM-ddTHH:mm:ss";

        public string TransactionId { get; set; }
        public string PublishedItemId { get; set; }
        public string Title { get; set; }
        public IEnumerable<PublishedItem> PublishedItems { get; set; }
        public ItemType ItemType { get; set; }
        public PublishState State { get; set; }
        public string PublishTarget { get; set; }
        public string Publication { get; set; }
        public string Server { get; set; }
        public User User { get; set; }
        public bool Published { get; set; }
        public DateTime TransactionDate { get; set; }
        public float ResolvingTime { get; set; }
        public float ExecutionTime { get; set; }
        public string DocId { get; set; }

        public override bool Equals(object? obj)
        {
            return (obj as PublishTransaction)?.TransactionId.Equals(TransactionId) ?? false;
        }

        public override int GetHashCode()
        {
            return TransactionId.GetHashCode();
        }
    }
}
