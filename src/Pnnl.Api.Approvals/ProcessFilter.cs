using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Pnnl.Api.Approvals
{
    /// <summary>
    /// Data class to hold the information used to filter items in the activity search
    /// </summary>
    public class ProcessFilter
    {
        /// <summary>
        /// A list used to filter activities by the assigned actors
        /// </summary>
        public List<string> ActorIdList { get; set; }

        /// <summary>
        /// A list used to filter processes by the originator of the request
        /// </summary>
        public List<string> OriginatorIdList { get; set; }

        /// <summary>
        /// A list used to filter processes by the beneficiary of the request
        /// </summary>
        public List<string> BeneficiaryIdList { get; set; }

        /// <summary>
        /// A list used to filter items based on the Action taken on them.
        /// </summary>
        /// <remarks>
        /// This should be one of the less common filter criteria
        /// </remarks>
        public List<ActorActionTaken> ActionTakenList { get; set; }

        /// <summary>
        /// A list used to filter items based on the activity state.
        /// </summary>
        /// <remarks>
        /// This should be one of the less common filter criteria
        /// </remarks>
        public List<ActivityState> ActivityStateList { get; set; }

        /// <summary>
        /// A list used to filter items based on the process state.
        /// </summary>
        /// <remarks>
        /// This should be one of the less common filter criteria
        /// </remarks>
        public List<ProcessState> ProcessStateList { get; set; }

        /// <summary>
        /// Filter the create date of the process based on the given range 
        /// </summary>
        /// <remarks>
        /// This should be one of the less common filter criteria, as that last change date will
        /// initially be the same as the create date.
        /// </remarks>
        public DateRange CreateDateRange { get; set; }

        /// <summary>
        /// Filter the last change date of the process based on the given range 
        /// </summary>
        public DateRange LastChangeDateRange { get; set; }

        /// <summary>
        /// A list used to filter processes by the document type
        /// </summary>
        public List<string> DocumentTypeList { get; set; }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
