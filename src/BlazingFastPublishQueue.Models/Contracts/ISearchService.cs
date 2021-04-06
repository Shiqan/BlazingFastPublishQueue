using System.Collections.Generic;
using System.Threading.Tasks;

namespace BlazingFastPublishQueue.Models.Contracts
{
    public interface ISearchService
    {
        Task<IEnumerable<string>> GetSuggestions(string query, string field);
        Task<PublishTransaction?> GetTransaction(string id);
        Task<SearchResult> GetTransactions(Filter filter, int page, int pageSize);
        Task<SearchResult> GetTransactions(Filter filter, int page, int pageSize, string? sortfield, SortDirection sortdirection);
        Task<AggregationResult> GetFilters();
    }
}
