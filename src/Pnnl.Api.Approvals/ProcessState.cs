using System;
using System.Collections.Generic;
using System.Text;

namespace Pnnl.Api.Approvals
{
    public abstract class ProcessState : Enumeration
    {
        public static ProcessState NotYetPending = new NULL();
        public static ProcessState Approved = new APPROVED();
        public static ProcessState Pending = new PENDING();
        public static ProcessState Terminated = new TERMINATED();

        protected ProcessState(int id, string name)
            : base(id, name)
        {
        }

        private class NULL : ProcessState
        {
            public NULL() : base(1, "NULL")
            { }
        }

        private class APPROVED : ProcessState
        {
            public APPROVED() : base(2, "APPROVED")
            { }
        }

        private class PENDING : ProcessState
        {
            public PENDING() : base(3, "PENDING")
            { }
        }

        private class TERMINATED : ProcessState
        {
            public TERMINATED() : base(4, "TERMINATED")
            { }
        }
    }
}
