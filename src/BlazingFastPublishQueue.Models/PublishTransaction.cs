using Nest;
using System;
using System.Collections.Generic;

namespace BlazingFastPublishQueue.Models
{
    public class PublishTransaction : IEquatable<PublishTransaction>
    {
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

        [Date(Format = "yyyy-MM-ddTHH:mm:ss")]
        public DateTime TransactionDate { get; set; }
        public float ResolvingTime { get; set; }
        public float ExcecutionTime { get; set; }

        public bool Equals(PublishTransaction other)
        {
            return other.TransactionId.Equals(TransactionId);
        }
    }

    public enum PublishState
    {
        None,
        WaitingForPublish,
        Rendering,
        WaitingForDeployment,
        Deploying,
        Success,
        Failed
    }

    public enum ItemType
    {
        None,
        Folder,
        StructureGroup,
        Component,
        Page,
        Keyword
    }
}
