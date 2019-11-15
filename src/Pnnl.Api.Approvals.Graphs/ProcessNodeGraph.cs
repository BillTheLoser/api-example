using GraphQL.Types;
using System;

namespace Pnnl.Api.Approvals.Graphs
{
    public class ProcessNodeResultGraph : ObjectGraphType<ProcessNodeResult>
    {
        public ProcessNodeResultGraph()
        {
            Name = "ProcessNode";
            Field(p => p.ProcessId).Description("The unique identifier assigned to this process");
            Field<ListGraphType<NodeGraph>>("nodes");
        }
    }
}
