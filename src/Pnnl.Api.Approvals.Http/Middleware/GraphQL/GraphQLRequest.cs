using Newtonsoft.Json.Linq;

namespace Pnnl.Api.Approvals.Http.Middleware.GraphQL
{
    public class GraphQLRequest
    {
        /// <summary>
        /// The name of graph operation
        /// </summary>
        public string OperationName { get; set; }
        /// <summary>
        /// The graphql query
        /// </summary>
        public string Query { get; set; }

        /// <summary>
        /// Query variables
        /// </summary>
        public JObject Variables { get; set; }
    }
}
