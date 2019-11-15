using System.Collections.Generic;

namespace Pnnl.Api.Approvals.Graphs
{
    public class Connection<T>
    {
        public PageInfo PageInfo { get; set; }
        public IEnumerable<T> Nodes { get; set; }
    }

    public class PageInfo
    {
        public int Total { get; set; }
        public int? Offset { get; set; }
        public int? Limit { get; set; }
    }
}
