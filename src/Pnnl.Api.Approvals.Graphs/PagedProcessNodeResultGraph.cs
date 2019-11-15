using GraphQL.Types;

namespace Pnnl.Api.Approvals.Graphs
{
    public class PagedProcessNodeResultGraph : ObjectGraphType<object>
    {
        public PagedProcessNodeResultGraph()
        {
            Name = "PagedProcessNode";
            Field<PageInfoGraph>("PageInfo");
            Field<ListGraphType<ProcessNodeResultGraph>>("Nodes");
        }
    }
}
