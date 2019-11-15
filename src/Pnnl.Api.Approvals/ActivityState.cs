namespace Pnnl.Api.Approvals
{
    /// <summary>
    /// Data class to hold the information used to filter items in the activity search
    /// </summary>
    public abstract class ActivityState : Enumeration
    {
        public static ActivityState NotYetPending = new NULL();
        public static ActivityState Complete = new COMPLETE();
        public static ActivityState Pending = new PENDING();
        public static ActivityState Escalated = new ESCALATED();

        protected ActivityState(int id, string name)
            : base(id, name)
        {
        }

        private class NULL : ActivityState
        {
            public NULL() : base(1, "NULL")
            { }
        }

        private class COMPLETE : ActivityState
        {
            public COMPLETE() : base(2, "COMPLETE")
            { }
        }

        private class PENDING : ActivityState
        {
            public PENDING() : base(3, "PENDING")
            { }
        }

        private class ESCALATED : ActivityState
        {
            public ESCALATED() : base(4, "PENDING ESCALATED")
            { }
        }
    }
}
