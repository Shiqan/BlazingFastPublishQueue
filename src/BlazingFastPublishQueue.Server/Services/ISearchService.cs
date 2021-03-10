using BlazingFastPublishQueue.Server.Models;
using BlazingFastPublishQueue.Server.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BlazingFastPublishQueue.Server.Services
{
    public interface ISearchService
    {
        Task<IEnumerable<string>> GetSuggestions(string query, string field);
        Task<PublishTransaction> GetTransaction(string id);
        Task<IEnumerable<PublishTransactionViewModel>> GetTransactions(Filter filter, int page, int pageSize);
    }
}