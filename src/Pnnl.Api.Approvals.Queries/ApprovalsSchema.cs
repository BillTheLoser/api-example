using GraphQL;
using GraphQL.Types;

namespace Pnnl.Api.Approvals.Queries
{
    public class ApprovalsSchema : Schema
    {
        public ApprovalsSchema(IDependencyResolver resolver) : base(resolver)
        {
            Query = resolver.Resolve<ApprovalsQuery>();
        }
    }
}
