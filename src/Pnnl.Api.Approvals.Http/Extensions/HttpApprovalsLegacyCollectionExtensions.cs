using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Pnnl.AppCore.Data.Sql;
using Pnnl.Api.Approvals.Data.Interfaces;
using Pnnl.Api.Approvals;
using Microsoft.Extensions.Caching.Memory;
//using Pnnl.Data.Http;
using Pnnl.Api.Approvals.Data.Http;
using System.Net.Http;

namespace Pnnl.Api.Approvals.Http.Extensions
{
    /// <summary>
    /// Extension methods for setting up Sql data services in an <see cref="IServiceCollection" />.
    /// </summary>
    public static class HttpApprovalsLegacyCollectionExtensions
    {
        
        /// <summary>
        /// Adds the Sql Server <see cref="IProcessFacade"/> services implementation to the specified <see cref="IServiceCollection" />.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection" /> to add service to.</param>
        /// <param name="configuration">The configuration being bound.</param>
        /// <returns>The <see cref="IServiceCollection" /> so that additional calls can be chained.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="services"/> collection is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="configuration"/> is <see langword="null"/>.</exception>
        public static IServiceCollection AddHttpRouteItemStore(this IServiceCollection services, IConfiguration configuration)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            services.AddOptions();

            services.TryAdd(ServiceDescriptor.Transient<IApprovalsLegacyStore>(provider =>
            {
                var cache = provider.GetRequiredService<IMemoryCache>();
                var logger = provider.GetRequiredService<ILogger<HttpApprovalsLegacyStore>>();
                var options = provider.GetRequiredService<IOptions<HttpRouteItemStoreOptions>>();
                var factory = provider.GetRequiredService<IHttpClientFactory>();
                var personIdentificationStore = provider.GetRequiredService<IPersonIdentificationStore>();

                //return new HttpRouteItemStore(logger, options, factory);
                return new HttpApprovalsLegacyStore (personIdentificationStore,logger, factory,options);
            }));

            services.Configure<HttpRouteItemStoreOptions>(configuration);

            return services;
        }
    }
}
