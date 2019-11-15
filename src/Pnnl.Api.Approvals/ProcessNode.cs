using System;
using System.Collections.Generic;
using System.Text;

namespace Pnnl.Api.Approvals
{
    public class ProcessNode
    {
        public int ProcessId { get; set; }

        public string NodeName { get; set; }

        public string NodeLabel { get; set; }

        public string NodeDataType { get; set; }

        public string NodeValue { get; set; }
    }

    public class ProcessNodeResult
    {
        public int ProcessId { get; set; }

        public IList<Node> Nodes { get; set; }
    }

    public class Node
    {
        public string NodeName { get; set; }

        public string NodeLabel { get; set; }

        public string NodeDataType { get; set; }

        public string NodeValue { get; set; }
    }
}
