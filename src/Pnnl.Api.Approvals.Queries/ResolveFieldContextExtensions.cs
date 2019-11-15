using GraphQL.Types;
using Pnnl.Api.Operations;
using System;

namespace Pnnl.Api.Approvals.Queries
{
    public static class ResolveFieldContextExtensions
    {
        public static Person GetCurrentUser(this ResolveFieldContext<object> context)
        {
            var userContext = context.UserContext as GraphQLUserContext;

            if (userContext?.User == null)
            {
                throw new Exception("Current user is not authenticated");
            }

            return userContext.User;
        }
    }
}
