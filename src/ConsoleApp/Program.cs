﻿using BlazingFastPublishQueue.Models;
using Bogus;
using Nest;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ConsoleApp
{
    class Program
    {
        private const string index = "publish_transactions";
        private static ElasticClient client;

        static async Task Main(string[] args)
        {
            var node = new Uri("http://localhost:9200/");
            var settings = new ConnectionSettings(node);
            client = new ElasticClient(settings);

            var n = Convert.ToInt32(args[0]);
            if (args.Length > 1 && args[1].Equals("--drop"))
            {
                await CreateIndex();
            }
            IndexBulk(publishTransactionGenerator.Generate(n));
        }

        private static async Task CreateIndex(bool dropIndex = false)
        {
            var existsResponse = await client.Indices.ExistsAsync(index);
            if (existsResponse.Exists && dropIndex)
            {
                await client.Indices.DeleteAsync(index);

                var createIndexResponse = await client.Indices.CreateAsync(index, c => c
                    .Map<PublishTransaction>(m => m.AutoMap())
                    .Map<PublishedItem>(m => m.AutoMap())
                );
            }
        }

        private static void IndexBulk(IEnumerable<PublishTransaction> transactions)
        {
            var bulkAllObservable = client.BulkAll(transactions, b => b
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
        private static Faker<PublishTransaction> publishTransactionGenerator => new Faker<PublishTransaction>()
                //Ensure all properties have rules. By default, StrictMode is false
                //Set a global policy by using Faker.DefaultStrictMode
                .StrictMode(true)
                .RuleFor(o => o.TransactionId, f => $"tcm:0-{ids++}-66560")
                .RuleFor(o => o.ItemId, f => $"tcm:{f.Random.Number(1, 100)}-{ids++}-{f.PickRandom(new int[4] { 2, 4, 16, 64 })}")
                .RuleFor(o => o.ItemType, f => f.PickRandom<ItemType>())
                .RuleFor(o => o.State, f => f.PickRandom<PublishState>())
                .RuleFor(o => o.PublishTarget, f => $"tcm:0-{f.Random.Number(1, 10)}-65537")
                .RuleFor(o => o.Publication, f => f.Name.FullName())
                .RuleFor(o => o.Server, f => f.Hacker.Noun())
                .RuleFor(o => o.User, f => f.Name.FullName())
                .RuleFor(o => o.Published, f => f.Random.Bool())
                .RuleFor(o => o.TransactionDate, f => f.Date.Recent(days: f.Random.Number(1, 10)))
                .RuleFor(o => o.ResolvingTime, f => f.Random.Float(0, 25))
                .RuleFor(o => o.ExcecutionTime, f => f.Random.Float(0, 25))
                .RuleFor(o => o.PublishedItems, f => publishedItemGenerator.Generate(f.Random.Number(1, 10)));
        
        private static Faker<PublishedItem> publishedItemGenerator => new Faker<PublishedItem>()
                .StrictMode(true)
                .RuleFor(o => o.ItemId, f => $"tcm:{f.Random.Number(1, 100)}-{ids++}-{f.PickRandom(new int[2] { 16, 64 })}")
                .RuleFor(o => o.ItemType, f => f.PickRandom<ItemType>())
                .RuleFor(o => o.Title, f => f.Hacker.Phrase());
    }
}
