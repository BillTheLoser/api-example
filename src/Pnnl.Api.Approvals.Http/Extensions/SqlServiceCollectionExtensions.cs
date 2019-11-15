using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Pnnl.AppCore.Data.Sql;
using Pnnl.Api.Approvals.Data.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Pnnl.Api.Approvals.Data.Sql;

namespace Pnnl.Api.Approvals.Http.Extensions
{
    /// <summary>
    /// Extension methods for setting up Sql data services in an <see cref="IServiceCollection" />.
    /// </summary>
    public static class SqlServiceCollectionExtensions
    {

        /// <summary>
        /// Adds the Sql Server <see cref="IProcessStore"/> services implementation to the specified <see cref="IServiceCollection" />.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection" /> to add service to.</param>
        /// <param name="configuration">The configuration being bound.</param>
        /// <returns>The <see cref="IServiceCollection" /> so that additional calls can be chained.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="services"/> collection is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="configuration"/> is <see langword="null"/>.</exception>
        public static IServiceCollection AddSqlProcessStore(this IServiceCollection services, IConfiguration configuration)
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

            services.TryAdd(ServiceDescriptor.Transient<IProcessStore>(provider =>
            {
                var cache = provider.GetRequiredService<IMemoryCache>();
                var logger = provider.GetRequiredService<ILogger<SqlProcessStore>>();
                var options = provider.GetRequiredService<IOptions<SqlProcessStoreOptions>>();
                var factory = provider.GetRequiredService<ISqlConnectionFactory>();
                var personId = provider.GetRequiredService<IPersonIdentificationStore>();

                return new SqlProcessStore(logger, options, factory, personId);
            }));

            services.Configure<SqlProcessStoreOptions>(configuration);

            return services;
        }
        /// <summary>
        /// Adds the Sql Server <see cref="IPersonIdentificationStore"/> services implementation to the specified <see cref="IServiceCollection" />.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection" /> to add service to.</param>
        /// <param name="configuration">The configuration being bound.</param>
        /// <returns>The <see cref="IServiceCollection" /> so that additional calls can be chained.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="services"/> collection is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="configuration"/> is <see langword="null"/>.</exception>
        public static IServiceCollection AddSqlPersonIdentificationStore(this IServiceCollection services, IConfiguration configuration)
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

            services.TryAdd(ServiceDescriptor.Transient<IPersonIdentificationStore>(provider =>
            {
                var cache = provider.GetRequiredService<IMemoryCache>();
                var logger = provider.GetRequiredService<ILogger<SqlPersonIdentificationStore>>();
                var options = provider.GetRequiredService<IOptions<SqlPersonIdentificationStoreOptions>>();
                var factory = provider.GetRequiredService<ISqlConnectionFactory>();

                return new SqlPersonIdentificationStore(cache, logger, options, factory);
            }));

            services.Configure<SqlPersonIdentificationStoreOptions>(configuration);

            return services;
        }
        /// <summary>
        /// Adds the Sql Server <see cref="IActivityStore"/> services implementation to the specified <see cref="IServiceCollection" />.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection" /> to add service to.</param>
        /// <param name="configuration">The configuration being bound.</param>
        /// <returns>The <see cref="IServiceCollection" /> so that additional calls can be chained.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="services"/> collection is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="configuration"/> is <see langword="null"/>.</exception>
        public static IServiceCollection AddSqlActivityStore(this IServiceCollection services, IConfiguration configuration)
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

            services.TryAdd(ServiceDescriptor.Transient<IActivityStore>(provider =>
            {
                var cache = provider.GetRequiredService<IMemoryCache>();
                var logger = provider.GetRequiredService<ILogger<SqlActivityStore>>();
                var options = provider.GetRequiredService<IOptions<SqlActivityStoreOptions>>();
                var factory = provider.GetRequiredService<ISqlConnectionFactory>();

                return new SqlActivityStore(logger, options, factory);
            }));

            services.Configure<SqlActivityStoreOptions>(configuration);

            return services;
        }

        public static IServiceCollection AddSqlSecurityStore(this IServiceCollection services, IConfiguration configuration)
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

            services.TryAdd(ServiceDescriptor.Transient<ISecurityStore>(provider =>
            {
                var cache = provider.GetRequiredService<IMemoryCache>();
                var logger = provider.GetRequiredService<ILogger<SqlSecurityStore>>();
                var options = provider.GetRequiredService<IOptions<SqlSecurityStoreOptions>>();
                var factory = provider.GetRequiredService<ISqlConnectionFactory>();

                return new SqlSecurityStore(cache, logger, options, factory);
            }));

            services.Configure<SqlSecurityStoreOptions>(configuration);

            return services;
        }

        public static IServiceCollection AddSqlProcessNodeStore(this IServiceCollection services, IConfiguration configuration)
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

            services.TryAdd(ServiceDescriptor.Transient<IProcessNodeStore>(provider =>
            {
                var logger = provider.GetRequiredService<ILogger<SqlProcessNodeStore>>();
                var options = provider.GetRequiredService<IOptions<SqlProcessNodeStoreOptions>>();
                var factory = provider.GetRequiredService<ISqlConnectionFactory>();

                return new SqlProcessNodeStore(logger, options, factory);
            }));

            services.Configure<SqlProcessNodeStoreOptions>(configuration);

            return services;
        }
    }
}
