using Microsoft.AspNetCore.Http;
using System;

namespace Pnnl.Api.Approvals.Http.Middleware.GraphQL
{
    public class GraphQLSettings
    {
        /// <summary>
        /// Relative path to api endpoint.
        /// </summary>
        public PathString Path { get; set; } = "/graphql";

        /// <summary>
        /// 
        /// </summary>
        public Func<HttpContext, object> BuildUserContext { get; set; }
    }
}
