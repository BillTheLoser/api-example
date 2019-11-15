using Microsoft.AspNetCore.Http;
using Pnnl.Api.Approvals.Http.Extensions;
using Pnnl.Api.Operations;

namespace Pnnl.Api.Approvals.Http.Extensions
{
    /// <summary>
    /// Provides a set of <see langword="static" /> methods that extend an <see cref="HttpContext"/>.
    /// </summary>
    public static class HttpContextExtensions
    {
        /// <summary>
        /// Gets the authenticated user.
        /// </summary>
        /// <param name="context">The HTTP-specific information about the current HTTP request.</param>
        /// <returns>The <see cref="Person"/> person associated with the claims principal of the current request.</returns>
        public static Person GetUser(this HttpContext context)
        {
            if (context == null)
            {
                return null;
            }

            if (!context.Items.TryGetValue(HttpContextItemKeys.ProcessApi.User, out object value))
            {
                return null;
            }

            return (Person) value;
        }
    }
}
