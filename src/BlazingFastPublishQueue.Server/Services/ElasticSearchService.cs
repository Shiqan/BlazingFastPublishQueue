using BlazingFastPublishQueue.Models;
using BlazingFastPublishQueue.Server.ViewModels;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Filter = BlazingFastPublishQueue.Server.Models.Filter;
using PublishTransaction = BlazingFastPublishQueue.Server.Models.PublishTransaction;

namespace BlazingFastPublishQueue.Server.Services
{
    public class ElasticSearchService : ISearchService
    {
        private readonly IElasticClient _client;

        public ElasticSearchService(IElasticClient client)
        {
            _client = client;
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

        public async Task<IEnumerable<PublishTransactionViewModel>> GetTransactions(Filter filter, int page, int pageSize)
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
                .Sort(q => q.Descending("timestamp"))
                .From(Math.Max((page - 1) * pageSize, 0))
                .Size(pageSize)
            );
            return response.ApiCall.Success ? response.Hits.Select(hit => new PublishTransactionViewModel
            {
                Id = hit.Id,
                Total = Convert.ToInt32(response.Total),
                PublishTransaction = hit.Source
            }) : Enumerable.Empty<PublishTransactionViewModel>();
        }

        public async Task<PublishTransaction> GetTransaction(string id)
        {
            var response = await _client.SourceAsync<PublishTransaction>(id);

            return response.Body;
        }

        /// <summary>
        /// https://www.elastic.co/guide/en/elasticsearch/client/net-api/current/writing-queries.html#structured-search
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        private static QueryContainer? CreateQueryContainer(Models.Filter filter)
        {
            QueryContainer? queryContainer = null;

            if (!string.IsNullOrEmpty(filter.Query))
            {
                queryContainer &= new SimpleQueryStringQuery()
                {
                    Fields = new Field("OriginCityName").And("DestCityName").And("FlightNum"),
                    Query = filter.Query,
                    DefaultOperator = Operator.Or,
                };
            }

            if (filter.State != null && !filter.State.Equals(PublishState.None))
            {
                queryContainer &= new TermQuery()
                {
                    Field = new Field("PublishState"),
                    Value = filter.State
                };
            }

            if (filter.ItemType != null && !filter.ItemType.Equals(ItemType.None))
            {
                queryContainer &= new TermQuery()
                {
                    Field = new Field("ItemType"),
                    Value = filter.ItemType
                };
            }

            if (!string.IsNullOrEmpty(filter.User))
            {
                queryContainer &= new TermQuery()
                {
                    Field = new Field("User"),
                    Value = filter.User
                };
            }

            if (!string.IsNullOrEmpty(filter.Publication))
            {
                queryContainer &= new TermQuery()
                {
                    Field = new Field("Publication"),
                    Value = filter.Publication
                };
            }

            if (filter.DateRange != null)
            {
                queryContainer &= new DateRangeQuery()
                {
                    Field = new Field("timestamp"),
                    GreaterThanOrEqualTo = filter.DateRange.Start,
                    LessThanOrEqualTo = filter.DateRange.End,
                };
            }

            return queryContainer;
        }
    }
}
