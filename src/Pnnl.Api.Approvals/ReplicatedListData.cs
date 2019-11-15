using System.Collections.Generic;

namespace Pnnl.Api.Approvals
{
    public class ReplicatedListData
    {
        public string ListType { get; set; }

        public IList<string> Values { get; set; }
    }
}
