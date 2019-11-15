using GraphQL.Types;

namespace Pnnl.Api.Approvals.Graphs.InputTypes
{
    public class ProcessFilterInput : InputObjectGraphType
    {
        public ProcessFilterInput()
        {
            Name = "ProcessFilter";
            Field<ListGraphType<StringGraphType>>("actorIdList");
            Field<ListGraphType<StringGraphType>>("originatorIdList");
            Field<ListGraphType<StringGraphType>>("beneficiaryIdList");
            Field<ListGraphType<StringGraphType>>("activityStateList");
            Field<ListGraphType<StringGraphType>>("processStateList");
            Field<DateTimeGraphType>("createDateStart");
            Field<DateTimeGraphType>("createDateEnd");
            Field<IntGraphType>("createDays");
            Field<DateTimeGraphType>("lastDateStart");
            Field<DateTimeGraphType>("lastDateEnd");
            Field<IntGraphType>("lastDays");
            Field<DateTimeGraphType>("lastChangeDateRange");
            Field<StringGraphType>("documentTypeList");
        }
    }
}
