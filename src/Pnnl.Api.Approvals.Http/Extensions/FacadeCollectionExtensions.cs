using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Pnnl.AppCore.Data.Sql;
using Pnnl.Api.Approvals.Data.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Pnnl.Api.Approvals.Data;

namespace Pnnl.Api.Approvals.Http.Extensions
{
    /// <summary>
    /// Extension methods for setting up Sql data services in an <see cref="IServiceCollection" />.
    /// </summary>
    public static class FacadeCollectionExtensions
    {

        /// <summary>
        /// Adds the Sql Server <see cref="IProcessFacade"/> services implementation to the specified <see cref="IServiceCollection" />.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection" /> to add service to.</param>
        /// <returns>The <see cref="IServiceCollection" /> so that additional calls can be chained.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="services"/> collection is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="configuration"/> is <see langword="null"/>.</exception>
        public static IServiceCollection AddProcessFacade(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.AddOptions();

            services.TryAdd(ServiceDescriptor.Transient<IProcessFacade>(provider =>
            {
                var cache = provider.GetRequiredService<IMemoryCache>();
                var logger = provider.GetRequiredService<ILogger<ProcessFacade>>();
                var factory = provider.GetRequiredService<ISqlConnectionFactory>();
                var personIdentificationStore = provider.GetRequiredService<IPersonIdentificationStore>();
                var processStore = provider.GetRequiredService<IProcessStore>();
                var approvalsLegacyStore = provider.GetRequiredService<IApprovalsLegacyStore>();
                var securityStore = provider.GetRequiredService<ISecurityStore>();

                return new ProcessFacade(logger, processStore, approvalsLegacyStore, securityStore);
            }));

            return services;
        }

        /// <summary>
        /// Adds the Sql Server <see cref="IActivityFacade"/> services implementation to the specified <see cref="IServiceCollection" />.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection" /> to add service to.</param>
        /// <returns>The <see cref="IServiceCollection" /> so that additional calls can be chained.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="services"/> collection is <see langword="null"/>.</exception>
        public static IServiceCollection AddActivityFacade(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.AddOptions();

            services.TryAdd(ServiceDescriptor.Transient<IActivityFacade>(provider =>
            {
                var cache = provider.GetRequiredService<IMemoryCache>();
                var logger = provider.GetRequiredService<ILogger<ActivityFacade>>();
                var factory = provider.GetRequiredService<ISqlConnectionFactory>();
                var personIdentificationStore = provider.GetRequiredService<IPersonIdentificationStore>();
                var activityStore = provider.GetRequiredService<IActivityStore>();
                var processStore = provider.GetRequiredService<IProcessStore>();
                var approvalsLegacyStore = provider.GetRequiredService<IApprovalsLegacyStore>();
                var securityStore = provider.GetRequiredService<ISecurityStore>();

                return new ActivityFacade(logger, activityStore, processStore, approvalsLegacyStore, personIdentificationStore, securityStore);
            }));

            return services;
        }
    }
}
