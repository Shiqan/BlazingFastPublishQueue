using BlazingFastPublishQueue.Models;
using BlazingFastPublishQueue.Models.Contracts;
using BlazingFastPublishQueue.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SolrNet;
using System;

namespace BlazingFastPublishQueue.ElasticSearch
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddElasticSearch(this IServiceCollection services, IConfiguration confugration)
        {
            var url = confugration["Solr:url"];
            services.AddSolrNet<PublishTransaction>(url);
            services.AddScoped<ISearchService, SolrSearchService>();
            return services;
        }
    }
}
