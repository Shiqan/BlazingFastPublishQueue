using BlazingFastPublishQueue.Models;
using SolrNet.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BlazingFastPublishQueue.Solr
{
    public class PublishTransactionWithSolrMapping
    {
        public PublishTransactionWithSolrMapping()
        {

        }

        public PublishTransactionWithSolrMapping(PublishTransaction t)
        {
            TransactionId = t.TransactionId;
            PublishedItemId = t.PublishedItemId;
            Title = t.Title;
            PublishedItems = t.PublishedItems.Select(i => $"{i.ItemId}___{i.Title}___{i.ItemType}");
            ItemType = t.ItemType;
            State = t.State;
            PublishTarget = t.PublishTarget;
            Publication = t.Publication;
            Server = t.Server;
            UserId = t.User.Id;
            UserName = t.User.Name;
            Published = t.Published;
            TransactionDate = t.TransactionDate;
            ResolvingTime = t.ResolvingTime;
            ExecutionTime = t.ExecutionTime;
        }

        public PublishTransaction ToPublishTransaction()
        {
            return new PublishTransaction
            {
                TransactionId = TransactionId,
                PublishedItemId = PublishedItemId,
                Title = Title,
                PublishedItems = PublishedItems.Select(i =>
                {
                    var s = i.Split("___");
                    _ = Enum.TryParse(s[2], out ItemType itemType);
                    return new PublishedItem
                    {
                        ItemId = s[0],
                        Title = s[1],
                        ItemType = itemType
                    };
                }),
                ItemType = ItemType,
                State = State,
                PublishTarget = PublishTarget,
                Publication = Publication,
                Server = Server,
                User = new User
                {
                    Id = UserId,
                    Name = UserName
                },
                Published = Published,
                TransactionDate = TransactionDate,
                ResolvingTime = ResolvingTime,
                ExecutionTime = ExecutionTime,
                //DocId = Id
                DocId = TransactionId
            };
        }

        [SolrUniqueKey("transactionId")]
        public string TransactionId { get; set; }

        [SolrField(fieldName: "publishedItemId")]
        public string PublishedItemId { get; set; }

        [SolrField(fieldName: "title")]
        public string Title { get; set; }

        [SolrField(fieldName: "publishedItems")]
        public IEnumerable<string> PublishedItems { get; set; }

        [SolrField(fieldName: "itemType")]
        public ItemType ItemType { get; set; }

        [SolrField(fieldName: "state")]
        public PublishState State { get; set; }

        [SolrField(fieldName: "publishTarget")]
        public string PublishTarget { get; set; }

        [SolrField(fieldName: "publication")]
        public string Publication { get; set; }

        [SolrField(fieldName: "server")]
        public string Server { get; set; }

        [SolrField(fieldName: "userId")]
        public string UserId { get; set; }

        [SolrField(fieldName: "userName")]
        public string UserName { get; set; }

        [SolrField(fieldName: "published")]
        public bool Published { get; set; }

        [SolrField(fieldName: "transactionDate")]
        public DateTime TransactionDate { get; set; }

        [SolrField(fieldName: "resolvingTime")]
        public float ResolvingTime { get; set; }

        [SolrField(fieldName: "executionTime")]
        public float ExecutionTime { get; set; }        
    
        [SolrField(fieldName: "id")]
        public string Id { get; set; }
    }
}
