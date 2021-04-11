using BlazingFastPublishQueue.Models.Contracts;
using BlazingFastPublishQueue.Solr;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SolrNet;

namespace BlazingFastPublishQueue.Services
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddPublishQueueSearch(this IServiceCollection services, IConfiguration configuration)
        {
            var url = configuration["Solr:url"];
            var index = configuration["Solr:index"];
            services.AddSolrNet<PublishTransactionWithSolrMapping>($"{url}{index}");
            services.AddScoped<ISearchService, SolrSearchService>();
            return services;
        }
    }
}
