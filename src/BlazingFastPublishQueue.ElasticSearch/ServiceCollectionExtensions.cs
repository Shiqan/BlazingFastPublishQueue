using BlazingFastPublishQueue.Models.Contracts;
using BlazingFastPublishQueue.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nest;
using System;

namespace BlazingFastPublishQueue.ElasticSearch
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddElasticSearch(this IServiceCollection services, IConfiguration confugration)
        {

            var url = confugration["ElasticSearch:url"];
            var defaultIndex = confugration["ElasticSearch:index"];

            var settings = new ConnectionSettings(new Uri(url))
                .DefaultIndex(defaultIndex);

            services.AddScoped<IElasticClient>(sp => new ElasticClient(settings));
            services.AddScoped<ISearchService, ElasticSearchService>();
            return services;
        }
    }
}
