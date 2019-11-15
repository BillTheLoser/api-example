using GraphQL.Types;

namespace Pnnl.Api.Approvals.Graphs
{
    public class NodeGraph : ObjectGraphType<Node>
    {
        public NodeGraph()
        {
            Field(n => n.NodeName);
            Field(n => n.NodeValue);
            Field(n => n.NodeLabel);
            Field(n => n.NodeDataType);
        }
    }
}
