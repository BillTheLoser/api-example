using Microsoft.AspNetCore.Builder;
using Pnnl.Api.Approvals.Http.Middleware;

namespace Pnnl.Api.Approvals.Http.Extensions
{
    /// <summary>
    /// Provides a set of <see langword="static" /> methods that provide registration of middleware components.
    /// </summary>
    public static class ApplicationBuilderExtensions
    {
        /// <summary>
        /// Adds the profile enricher to the request execution pipeline.
        /// </summary>s
        /// <param name="app">The application request pipeline to add the middleware to.</param>
        /// <returns>The <see cref="IApplicationBuilder"/> after the middleware has been added.</returns>
        public static IApplicationBuilder UseUserEnrichment(this IApplicationBuilder app)
        {
            return app?.UseMiddleware<ContextEnricher>();
        }
    }
}
