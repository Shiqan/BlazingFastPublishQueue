using BlazingFastPublishQueue.Models;
using BlazingFastPublishQueue.Server.Models;
using Microsoft.Extensions.Logging;
using MudBlazor;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Filter = BlazingFastPublishQueue.Server.Models.Filter;

namespace BlazingFastPublishQueue.Server.Services
{
    public class ElasticSearchService : ISearchService
    {
        private readonly IElasticClient _client;
        private readonly ILogger _logger;

        public ElasticSearchService(IElasticClient client, ILogger<ElasticSearchService> logger)
        {
            _client = client;
            _logger = logger;
        }

        public async Task<IEnumerable<string>> GetSuggestions(string query, string field)
        {
            var response = await _client.SearchAsync<PublishTransaction>(
                s => s
                    .Suggest(su => su
                        .Completion("suggestions", c => c
                            .Prefix(query)
                            .Field(new Field(field))
                            .SkipDuplicates()
                    )
                )
            );

            return response.ApiCall.Success ? response.Suggest["suggestions"].SelectMany(x => x.Options.Select(o => o.Text)) : Enumerable.Empty<string>();
        }

        public async Task<SearchResult> GetTransactions(Filter filter, int page, int pageSize, string? sortfield, SortDirection sortdirection)
        {
            var response = await _client.SearchAsync<PublishTransaction>(
                s => s.Query(q =>
                {
                    var queryContainer = CreateQueryContainer(filter);
                    if (queryContainer is null)
                    {
                        return q.MatchAll();
                    }
                    return q.Bool(b => b.Filter(queryContainer));
                })
                .Sort(q =>
                {
                    var s = !string.IsNullOrEmpty(sortfield) ? sortfield : "transactionDate";
                    return sortdirection switch
                    {
                        SortDirection.Ascending => q.Ascending(s),
                        _ => q.Descending(s),
                    };
                })
                .From(Math.Max(page * pageSize, 0))
                .Size(pageSize)
            );

            _logger.LogDebug($"Search {response.ApiCall.Uri} has status code {response.ApiCall.HttpStatusCode}");

            return response.ApiCall.Success ? new SearchResult
            {
                TotalItems = Convert.ToInt32(response.Total),
                Items = response.Hits.Select(hit =>
                {
                    var t = hit.Source;
                    t.DocId = hit.Id;
                    return t;
                })
            } : new SearchResult();
        }

        public async Task<SearchResult> GetTransactions(Filter filter, int page, int pageSize)
        {
            return await GetTransactions(filter, page, pageSize, null, SortDirection.None);
        }

        public async Task<PublishTransaction?> GetTransaction(string id)
        {
            var response = await _client.SourceAsync<PublishTransaction>(id);

            return response.ApiCall.Success ? response.Body : null;
        }

        public async Task<AggregationResult> GetFilters()
        {
            var response = await _client.SearchAsync<PublishTransaction>(s =>
                s.Aggregations(a => a
                    .Terms("publishTargets", t => t.Field("publishTarget.keyword"))
                    .Terms("servers", t => t.Field("server.keyword"))
                    .Terms("publications", t => t.Field("publication.keyword"))
                )
                .Size(0)
            );

            return response.ApiCall.Success ? new AggregationResult
            {
                { "publishTargets", response.Aggregations.Terms("publishTargets").Buckets
                    .Select(e => new AggregationBucket
                    {
                        Key = e.Key,
                        Count = e.DocCount
                    })
                },
                {  "servers", response.Aggregations.Terms("servers").Buckets
                    .Select(e => new AggregationBucket
                    {
                        Key = e.Key,
                        Count = e.DocCount
                    })
                },
                {"publications", response.Aggregations.Terms("publications").Buckets
                    .Select(e => new AggregationBucket
                    {
                        Key = e.Key,
                        Count = e.DocCount
                    })
                }
            } : new AggregationResult();
        }

        /// <summary>
        /// https://www.elastic.co/guide/en/elasticsearch/client/net-api/current/writing-queries.html#structured-search
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        private static QueryContainer? CreateQueryContainer(Filter filter)
        {
            QueryContainer? queryContainer = null;

            if (!string.IsNullOrEmpty(filter.Query))
            {
                queryContainer &= new SimpleQueryStringQuery()
                {
                    Fields = new Field("title").And("publishedItemId"),
                    Query = filter.Query,
                    DefaultOperator = Operator.And,
                };
            }

            if (filter.State != null && !filter.State.Equals(PublishState.None))
            {
                queryContainer &= new TermQuery()
                {
                    Field = new Field("state"),
                    Value = filter.State
                };
            }

            if (filter.ItemType != null && !filter.ItemType.Equals(ItemType.None))
            {
                queryContainer &= new TermQuery()
                {
                    Field = new Field("itemType"),
                    Value = filter.ItemType
                };
            }

            if (!string.IsNullOrEmpty(filter.User))
            {
                queryContainer &= new TermQuery()
                {
                    Field = new Field("user"),
                    Value = filter.User
                };
            }

            if (!string.IsNullOrEmpty(filter.Server) && filter.Server != "None")
            {
                queryContainer &= new TermQuery()
                {
                    Field = new Field("server"),
                    Value = filter.Server
                };
            }

            if (!string.IsNullOrEmpty(filter.Publication) && filter.Publication != "None")
            {
                queryContainer &= new TermQuery()
                {
                    Field = new Field("publication"),
                    Value = filter.Publication
                };
            }

            if (!string.IsNullOrEmpty(filter.PublishTarget) && filter.PublishTarget != "None")
            {
                queryContainer &= new TermQuery()
                {
                    Field = new Field("publishTarget.keyword"),
                    Value = filter.PublishTarget
                };
            }

            if (filter.Published is not null)
            {
                queryContainer &= new TermQuery()
                {
                    Field = new Field("published"),
                    Value = filter.Published
                };
            }

            if (filter.DateRange != null)
            {
                queryContainer &= new DateRangeQuery()
                {
                    Field = new Field("transactionDate"),
                    GreaterThanOrEqualTo = filter.DateRange.Start,
                    LessThanOrEqualTo = filter.DateRange.End,
                };
            }

            return queryContainer;
        }
    }
}
