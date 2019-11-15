using GraphQL.Types;
using Pnnl.Api.Approvals.Data.Interfaces;

namespace Pnnl.Api.Approvals.Graphs
{
    public class ActivityGraph : ObjectGraphType<Activity>
    {
        public ActivityGraph(IPersonStore personStore)
        {
            Name = "Activity";
            Field(a => a.ActivityId);
            Field(a => a.ActivityName);
            Field(a => a.PendingDateTime, nullable: true);
            Field(a => a.LastChangeDateTime);
            Field(a => a.ActivityState, nullable: true);
            Field(a => a.ActivityStatus, nullable: true);
            Field(a => a.Comment, nullable: true);
            Field(a => a.ActedActorId, nullable: true);
            Field(a => a.IsGhost);
            Field(a => a.IsAdhoc);
            //Field(a => a.Actors);

            FieldAsync<UserGraph>("actedHanfordId", "The hanford id of the person who acted on the activity",
               resolve: async (context) => await personStore.GetByIdAsync(context.Source.ActedHanfordId, context.CancellationToken));            
        }
    }
}
