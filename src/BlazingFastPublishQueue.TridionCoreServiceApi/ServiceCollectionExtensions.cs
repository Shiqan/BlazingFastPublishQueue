using BlazingFastPublishQueue.Models.Contracts;
using BlazingFastPublishQueue.TridionCoreServiceApi;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BlazingFastPublishQueue.Services
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddTridion(this IServiceCollection services, IConfiguration confugration)
        {
            var url = confugration["Tridion:url"];

            services.AddScoped<ITridionCoreService, TridionCoreService>();
            return services;
        }
    }
}
