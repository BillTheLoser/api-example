using GraphQL.Types;
using Pnnl.Api.Approvals.Data.Interfaces;
using System.Linq;

namespace Pnnl.Api.Approvals.Graphs
{
    public class ProcessGraph : ObjectGraphType<Process>
    {
        public ProcessGraph(IPersonStore personStore, IActivityFacade activityFacade)
        {
            Name = "Process";
            Field(p => p.ProcessId).Description("The unique identifier assigned to this process");
            Field(p => p.ProcessDefinitionId).Description("The unique identifier assigned to process definition that created this resource");
            Field(p => p.DocumentTypeName, nullable: true).Description("The document type name of this resource");
            Field(p => p.DocumentId, nullable:true).Description("The document id of the item provided by the user or the source system");
            Field(p => p.DocumentTitle, nullable:true).Description("The title of the document");
            Field(p => p.ProcessState, nullable: true).Description("The current state of the process");
            Field(p => p.ProcessStatus, nullable: true).Description("The current status of the process");
            Field(p => p.CreateDateTime).Description("The created date of the process");
            Field(p => p.LastChangeDateTime).Description("The last changed date of the process");

            FieldAsync<UserGraph>("originatorHanfordId", "The person who routed the process",
                resolve: async (context) => await personStore.GetByIdAsync(context.Source.OriginatorHanfordId, context.CancellationToken));
            FieldAsync<UserGraph>("beneficiaryHanfordId", "The beneficiary of the process",
                resolve: async (context) => await personStore.GetByIdAsync(context.Source.BeneficiaryHanfordId, context.CancellationToken));
            FieldAsync<UserGraph>("lastChangeHanfordId", "The person who last changed the process",
                resolve: async (context) => await personStore.GetByIdAsync(context.Source.LastChangeHanfordId, context.CancellationToken));
            FieldAsync<ListGraphType<ActivityGraph>>(
                name: "activities",
                description: "The list of activities in this process",
                arguments: new QueryArguments
                {
                    new QueryArgument<IntGraphType>{Name="offset"},
                    new QueryArgument<IntGraphType>{Name="limit"}
                },
                resolve: async (context) =>
                {
                    var offset = context.GetArgument<int?>("offset");
                    var limit = context.GetArgument<int?>("limit");

                    var activityIds = context
                                        .Source
                                        .Activities
                                        .Values
                                        .Select(a => a.ActivityId)
                                        .ToList();

                    return await activityFacade.GetAsync(activityIds, offset, limit, context.CancellationToken);
                });
        }
    }
}
