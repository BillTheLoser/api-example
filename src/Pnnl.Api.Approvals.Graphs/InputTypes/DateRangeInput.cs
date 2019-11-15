using GraphQL.Types;

namespace Pnnl.Api.Approvals.Graphs.InputTypes
{
    public class DateRangeInput : InputObjectGraphType<DateRange>
    {
        public DateRangeInput()
        {
            Name = "DateRange";
            Field(d => d.Start, nullable: false);
            Field(d => d.End, nullable: false);
            Field(d => d.Days);
        }
    }
}
