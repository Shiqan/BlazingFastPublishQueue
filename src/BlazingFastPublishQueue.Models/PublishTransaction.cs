using Nest;
using System;
using System.Collections.Generic;

namespace BlazingFastPublishQueue.Models
{
    public class PublishTransaction
    {
        public string TransactionId { get; set; }
        public string ItemId { get; set; }
        public IEnumerable<PublishedItem> PublishedItems { get; set; }
        public ItemType ItemType { get; set; }
        public PublishState State { get; set; }
        public string PublishTarget { get; set; }
        [Completion]
        public string PublishTargetCompletion => PublishTarget;
        public string Publication { get; set; }
        [Completion]
        public string PublicationCompletion => Publication;
        public string Server { get; set; }
        [Completion]
        public string ServerCompletion => Server;
        public string User { get; set; }
        [Completion]
        public string UserCompletion => User;
        public bool Published { get; set; }
        public DateTime TransactionDate { get; set; }
        public float ResolvingTime { get; set; }
        public float ExcecutionTime { get; set; }
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
