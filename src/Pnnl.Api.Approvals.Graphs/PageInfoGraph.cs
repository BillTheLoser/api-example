using GraphQL.Types;

namespace Pnnl.Api.Approvals.Graphs
{
    public class PageInfoGraph : ObjectGraphType<object>
    {
        public PageInfoGraph()
        {
            Name = "PageInfo";
            Field<IntGraphType>("Total");
            Field<IntGraphType>("Offset");
            Field<IntGraphType>("Limit");
        }
    }
}
