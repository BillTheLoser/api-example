namespace Pnnl.Api.Approvals
{
    /// <summary>
    /// Data class to hold the information for an actor on an <see cref="Activity"/>
    /// </summary>
    public class Actor
    {
        /// <summary>
        /// The unique identifier for the actor entry in the approvals system.
        /// </summary>
        public int ActorId { get; set; }

        /// <summary>
        /// The uniqure identifier for the activity that this actor assignment is associated with.
        /// </summary>
        public int ActivityId { get; set; }

        /// <summary>
        /// The unique identifier of the actor criteria that generated this actor assignment.
        /// </summary>
        public int ActorCriteriaId { get; set; }

        /// <summary>
        /// The hanford id of the actor
        /// </summary>
        public string ActorHanfordId { get; set; }

        /// <summary>
        /// The actor type of the assignment.
        /// </summary>
        public string ActorType { get; set; }

        /// <summary>
        /// The original actor, if this actor type is a delegate.
        /// </summary>
        public string DelegatorHanfordId { get; set; }
    }
}
