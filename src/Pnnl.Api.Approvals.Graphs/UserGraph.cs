using GraphQL.Types;
using Pnnl.Api.Operations;

namespace Pnnl.Api.Approvals.Graphs
{
    public class UserGraph : ObjectGraphType<Person>
    {
        public UserGraph()
        {
            Name = "User";
            Field(p => p.Name);
            Field(p => p.FirstName);
            Field(p => p.LastName);
            Field(p => p.EmployeeId);
            Field(p => p.Network.Id, nullable: true);
            Field(p => p.Network.Username);
            Field(p => p.EmailAddress);
            Field(p => p.DepartmentId);
        }
    }
}
