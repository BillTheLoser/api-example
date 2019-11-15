using GraphQL.Types;
using Pnnl.Api.Approvals.Data.Interfaces;

namespace Pnnl.Api.Approvals.Graphs
{
    public class ActorGraph: ObjectGraphType<Actor>
    {
        public ActorGraph(IPersonStore personStore)
        {
            Name = "Actor";
            Field(a => a.ActorId);
            Field(a => a.ActivityId);
            Field(a => a.ActorCriteriaId);
            Field(a => a.ActorHanfordId);
            Field(a => a.ActorType);

            FieldAsync<UserGraph>("delegatorHanfordId", "The original actor, if the actor type is delegate",
               resolve: async (context) => await personStore.GetByIdAsync(context.Source.DelegatorHanfordId, context.CancellationToken));
        }
    }
}
