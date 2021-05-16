using BlazingFastPublishQueue.Models;
using BlazingFastPublishQueue.Solr;
using Bogus;
using CommonServiceLocator;
using Nest;
using SolrNet;
using SolrNet.Attributes;
using SolrNet.Commands.Parameters;
using SolrNet.Mapping;
using SolrNet.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace ConsoleApp
{
    class Program
    {
        private const string index = "publish_transactions";
        private static ElasticClient elasticClient;
        private static ISolrOperations<PublishTransactionWithSolrMapping> solrClient;

        static async Task Main(string[] args)
        {
            var node = new Uri("http://localhost:9200/");
            var settings = new ConnectionSettings(node)
                .DefaultIndex(index);
            elasticClient = new ElasticClient(settings);


            //var mapper = new MappingManager();
            //var property = typeof(PublishTransaction).GetProperty("TransactionId");
            //mapper.Add(property, "transactionId");
            //mapper.SetUniqueKey(property);
            //mapper.Add(typeof(PublishTransaction).GetProperty("PublishedItemId"), "publishedItemId");
            //mapper.Add(typeof(PublishTransaction).GetProperty("Title"), "title");
            ////mapper.Add(typeof(PublishTransaction).GetProperty("PublishedItems"), "publishedItems");
            ////mapper.Add(typeof(PublishTransaction).GetProperty("ItemType"), "itemType");
            ////mapper.Add(typeof(PublishTransaction).GetProperty("State"), "state");
            //mapper.Add(typeof(PublishTransaction).GetProperty("PublishTarget"), "publishTarget");
            //mapper.Add(typeof(PublishTransaction).GetProperty("Publication"), "publication");
            //mapper.Add(typeof(PublishTransaction).GetProperty("Server"), "server");
            ////mapper.Add(typeof(PublishTransaction).GetProperty("User"), "user");
            //mapper.Add(typeof(PublishTransaction).GetProperty("Published"), "published");
            //mapper.Add(typeof(PublishTransaction).GetProperty("TransactionDate"), "transactionDate");
            //mapper.Add(typeof(PublishTransaction).GetProperty("ResolvingTime"), "resolvingTime");
            //mapper.Add(typeof(PublishTransaction).GetProperty("ExcecutionTime"), "excecutionTime");
            //mapper.Add(typeof(User).GetProperty("Id"), "user.id");
            //mapper.Add(typeof(User).GetProperty("Name"), "user.name");
            //mapper.Add(typeof(PublishedItem).GetProperty("ItemId"), "publishedItem.itemId");
            //mapper.Add(typeof(PublishedItem).GetProperty("Title"), "publishedItem.title");
            ////mapper.Add(typeof(PublishedItem).GetProperty("ItemType"), "publishedItem.itemType");
            //var container = new Container(Startup.Container);

            //Startup.Container.RemoveAll<IReadOnlyMappingManager>();
            //Startup.Container.Register<IReadOnlyMappingManager>(c => mapper);

            Startup.Init<PublishTransactionWithSolrMapping>($"http://localhost:8983/solr/{index}");
            solrClient = ServiceLocator.Current.GetInstance<ISolrOperations<PublishTransactionWithSolrMapping>>();


            var n = Convert.ToInt32(args[0]);
            var drop = args.Length > 1 && args[1].Equals("--drop");

            //await CreateIndex(dropIndex: drop);

            var t = publishTransactionGenerator.Generate(n);
            //IndexBulkElastic(t);
            await IndexBulkSolr(t);

            //var response = await solrClient.QueryAsync(new SolrQueryByField("transactionId", "tcm:0-0-66560"), new QueryOptions
            //{
            //    Rows = 1
            //});
            //Console.WriteLine(response.First().Title);
        }


        private static async Task CreateIndex(bool dropIndex = false)
        {
            var existsResponse = await elasticClient.Indices.ExistsAsync(index);
            if (!existsResponse.Exists || dropIndex)
            {
                await elasticClient.Indices.DeleteAsync(index);

                var createIndexResponse = await elasticClient.Indices.CreateAsync(index, c => c
                    .Map<PublishTransaction>(m => m.AutoMap())
                    .Map<PublishedItem>(m => m.AutoMap())
                    .Map<User>(m => m.AutoMap())
                );

            }
        }

        private static async Task IndexBulkSolr(IEnumerable<PublishTransaction> transactions)
        {
            var t = transactions.Select(i => new PublishTransactionWithSolrMapping(i));
            await solrClient.AddRangeAsync(t);
            await solrClient.CommitAsync();
        }

        private static void IndexBulkElastic(IEnumerable<PublishTransaction> transactions)
        {
            var bulkAllObservable = elasticClient.BulkAll(transactions, b => b
                .Index(index)
                // how long to wait between retries
                .BackOffTime("30s")
                // how many retries are attempted if a failure occurs
                .BackOffRetries(2)
                // refresh the index once the bulk operation completes
                .RefreshOnCompleted()
                // how many concurrent bulk requests to make
                .MaxDegreeOfParallelism(Environment.ProcessorCount)
                // number of items per bulk request
                .Size(100)
            )
            // Perform the indexing, waiting up to 15 minutes. 
            // Whilst the BulkAll calls are asynchronous this is a blocking operation
            .Wait(TimeSpan.FromMinutes(15), next =>
            {
                Console.WriteLine(next.Page);
            });
        }

        private static int ids = 0;
        private static int userIds = 0;
        private static string[] servers = new string[3] { "server01", "server02", "server03" };
        private static string[] publishtargets = new string[3] { "staging", "verification", "live" };
        private static string[] publications = new string[5] { "publication01", "publication02", "publication03", "publication04", "publication05" };

        private static IEnumerable<User> users = UserGenerator.Generate(1000);
        private static Faker<PublishTransaction> publishTransactionGenerator => new Faker<PublishTransaction>()
                //Ensure all properties have rules. By default, StrictMode is false
                //Set a global policy by using Faker.DefaultStrictMode
                .StrictMode(true)
                .Ignore(o => o.DocId)
                .RuleFor(o => o.TransactionId, f => $"tcm:0-{ids++}-66560")
                .RuleFor(o => o.PublishedItemId, f => $"tcm:{f.Random.Number(1, 100)}-{ids++}-{f.PickRandom(new int[4] { 2, 4, 16, 64 })}")
                .RuleFor(o => o.Title, f => f.Hacker.Phrase())
                .RuleFor(o => o.ItemType, f => f.PickRandomWithout(ItemType.None))
                .RuleFor(o => o.State, f => f.PickRandomWithout(PublishState.None))
                .RuleFor(o => o.PublishTarget, f => f.PickRandom(publishtargets))
                .RuleFor(o => o.Publication, f => f.PickRandom(publications))
                .RuleFor(o => o.Server, f => f.PickRandom(servers))
                .RuleFor(o => o.User, f => f.PickRandom(users))
                .RuleFor(o => o.Published, f => f.Random.Bool())
                .RuleFor(o => o.TransactionDate, f => f.Date.Recent(days: f.Random.Number(1, 10)))
                .RuleFor(o => o.ResolvingTime, f => f.Random.Float(0, 25))
                .RuleFor(o => o.ExecutionTime, f => f.Random.Float(0, 25))
                .RuleFor(o => o.PublishedItems, f => PublishedItemGenerator.Generate(f.Random.Number(1, 10)));

        private static Faker<PublishedItem> PublishedItemGenerator => new Faker<PublishedItem>()
                .StrictMode(true)
                .RuleFor(o => o.ItemId, f => $"tcm:{f.Random.Number(1, 100)}-{ids++}-{f.PickRandom(new int[2] { 16, 64 })}")
                .RuleFor(o => o.ItemType, f => f.PickRandomWithout(ItemType.None))
                .RuleFor(o => o.Title, f => f.Hacker.Phrase());

        private static Faker<User> UserGenerator => new Faker<User>()
                .StrictMode(true)
                .RuleFor(o => o.Id, f => $"tcm:0-{userIds++}-65552")
                .RuleFor(o => o.Name, f => f.Name.FullName());
    }


}
