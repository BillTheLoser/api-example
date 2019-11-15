using GraphQL.Types;

namespace Pnnl.Api.Approvals.Graphs.InputTypes
{
    public class ActorActionTakenEnumInput : EnumerationGraphType<ActorActionTaken>
    {
        public ActorActionTakenEnumInput()
        {
            Name = "ActorActionTaken";
        }
    }
}
