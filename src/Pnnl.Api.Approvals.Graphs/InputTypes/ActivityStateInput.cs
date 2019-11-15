using GraphQL.Types;

namespace Pnnl.Api.Approvals.Graphs.InputTypes
{
    public class ActivityStateInput: InputObjectGraphType<ActivityState>
    {
        public ActivityStateInput()
        {
            Name = "ActivityState";
            Field(a => a.Id, nullable: true).Description("The enum value of this input.");
            Field(a => a.Name, nullable: true).Description("The enum name of this input.");
            Field<ActivityStateInput>("notYetPending");
            Field<ActivityStateInput>("complete");
            Field<ActivityStateInput>("pending");
            Field<ActivityStateInput>("escalated");
        }
    }
}
