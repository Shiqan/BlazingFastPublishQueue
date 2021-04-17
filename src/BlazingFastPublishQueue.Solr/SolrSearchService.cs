
using BlazingFastPublishQueue.Models;
using BlazingFastPublishQueue.Models.Contracts;
using BlazingFastPublishQueue.Solr;
using Microsoft.Extensions.Logging;
using SolrNet;
using SolrNet.Commands.Parameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Filter = BlazingFastPublishQueue.Models.Filter;

namespace BlazingFastPublishQueue.Services
{
    public class SolrSearchService : ISearchService
    {
        private readonly ISolrReadOnlyOperations<PublishTransactionWithSolrMapping> _client;
        private readonly ILogger _logger;

        public SolrSearchService(ISolrReadOnlyOperations<PublishTransactionWithSolrMapping> client, ILogger<SolrSearchService> logger)
        {
            _client = client;
            _logger = logger;
        }

        public async Task<IEnumerable<string>> GetSuggestions(string query, string field)
        {
            // TODO: use suggest endpoint...
            if (query is null)
            {
                return Enumerable.Empty<string>();
            }
            var response = await _client.QueryAsync(query, new QueryOptions
            {
                SpellCheck = new SpellCheckingParameters { Count = 10 }
            });
            return response.SpellChecking.FirstOrDefault() is not null ? response.SpellChecking.First().Suggestions : Enumerable.Empty<string>();
        }

        public async Task<SearchResult> GetTransactions(Filter filter, int page, int pageSize, string? sortfield, SortDirection sortdirection)
        {
            var s = !string.IsNullOrEmpty(sortfield) ? sortfield : "transactionDate";

            var response = await _client.QueryAsync(CreateQueryContainer(filter), new QueryOptions
            {
                Rows = pageSize,
                StartOrCursor = new StartOrCursor.Start(Math.Max(page * pageSize, 0)),
                OrderBy = GetSortOrders(sortfield, sortdirection)
            });

            return response.NumFound > 0 ? new SearchResult
            {
                TotalItems = response.NumFound,
                Items = response.Select(hit => hit.ToPublishTransaction())
            } : new SearchResult();
        }

        public async Task<SearchResult> GetTransactions(Filter filter, int page, int pageSize)
        {
            return await GetTransactions(filter, page, pageSize, null, SortDirection.None);
        }

        public async Task<PublishTransaction?> GetTransaction(string id)
        {
            // TODO: use id (and get endpoint), or use transaction id in elastic as well...
            var response = await _client.QueryAsync(new SolrQueryByField("transactionId", id), new QueryOptions
            {
                Rows = 1
            });

            return response.FirstOrDefault()?.ToPublishTransaction();
        }

        public async Task<AggregationResult> GetFilters()
        {
            var response = await _client.QueryAsync(SolrQuery.All, new QueryOptions
            {
                Rows = 0,
                Facet = new FacetParameters
                {
                    Queries = new[] {
                            new SolrFacetFieldQuery("publishTarget"),
                            new SolrFacetFieldQuery("server"),
                            new SolrFacetFieldQuery("publication"),
                    }
                }
            });

            return response.FacetFields is not null ? new AggregationResult
            {
                { "publishTargets", response.FacetFields["publishTarget"]
                    .Select(e => new AggregationBucket
                    {
                        Key = e.Key,
                        Count = e.Value
                    })
                },
                { "servers", response.FacetFields["server"]
                    .Select(e => new AggregationBucket
                    {
                        Key = e.Key,
                        Count = e.Value
                    })
                },
                { "publications", response.FacetFields["publication"]
                    .Select(e => new AggregationBucket
                    {
                        Key = e.Key,
                        Count = e.Value
                    })
                },
            } : new AggregationResult();
        }

        private static ICollection<SortOrder> GetSortOrders(string? sortfield, SortDirection sortdirection)
        {
            var s = !string.IsNullOrEmpty(sortfield) ? sortfield : "transactionDate";
            return new[] { sortdirection switch
                {
                    SortDirection.Ascending => new SortOrder(s, Order.ASC),
                    _ => new SortOrder(s, Order.DESC),
                }
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        private static ISolrQuery CreateQueryContainer(Filter filter)
        {
            // TODO: use filter queries
            AbstractSolrQuery? queryContainer = SolrQuery.All;

            if (!string.IsNullOrEmpty(filter.Query))
            {
                queryContainer &= new SolrQueryByField("title", filter.Query) { Quoted = false } || new SolrQueryByField("publishedItemId", filter.Query);
            }

            if (filter.State != null && !filter.State.Equals(PublishState.None))
            {
                queryContainer &= new SolrQueryByField("state", filter.State.ToString());
            }

            if (filter.ItemType != null && !filter.ItemType.Equals(ItemType.None))
            {
                queryContainer &= new SolrQueryByField("itemType", filter.ItemType.ToString());
            }

            if (!string.IsNullOrEmpty(filter.User))
            {
                queryContainer &= new SolrQueryByField("userName", filter.User);
            }

            if (!string.IsNullOrEmpty(filter.Server) && filter.Server != "None")
            {
                queryContainer &= new SolrQueryByField("server", filter.Server);
            }

            if (!string.IsNullOrEmpty(filter.Publication) && filter.Publication != "None")
            {
                queryContainer &= new SolrQueryByField("publication", filter.Publication);
            }

            if (!string.IsNullOrEmpty(filter.PublishTarget) && filter.PublishTarget != "None")
            {
                queryContainer &= new SolrQueryByField("publishTarget", filter.PublishTarget);
            }

            if (filter.Published is not null)
            {
                queryContainer &= new SolrQueryByField("published", filter.Published.ToString());
            }

            if (filter.DateRange != null)
            {
                queryContainer &= new SolrQueryByRange<DateTime?>("transactionDate", filter.DateRange.Start, filter.DateRange.End, true);
            }

            return queryContainer;
        }
    }
}
