namespace Pnnl.Api.Approvals
{
    public class TerminateProcessResponse
    {
        public int ProcessId { get; set; }

        public int ResultId { get; set; }

        public string Status { get; set; }

        public string State { get; set; }
    }
}
