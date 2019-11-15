using System;
using System.Collections.Generic;

namespace Pnnl.Api.Approvals
{
    /// <summary>
    /// Data class to hold the information for an activity in an Approvals <see cref="Process"/>
    /// </summary>
    public class Activity
    {
        /// <summary>
        /// The unique identifier for this activity within the approvals system.
        /// </summary>
        public int ActivityId { get; set; }

        /// <summary>
        /// The human readable name of the activity.
        /// </summary>
        public string ActivityName { get; set; }

        /// <summary>
        /// The date the activity was moved into the pending state.
        /// </summary>
        public DateTime PendingDateTime { get; set; }

        /// <summary>
        /// The date the activity was last updated
        /// </summary>
        public DateTime LastChangeDateTime { get; set; }

        /// <summary>
        /// The current state for the activity
        /// </summary>
        /// <value>NULL|COMPLETE|PEDNING|PENDING ESCALATED</value>
        public string ActivityState { get; set; }

        /// <summary>
        /// The current state for the activity
        /// </summary>
        /// <value>NULL|ACCEPTED|ACKNOWLEDGED|REJECTED</value>
        public string ActivityStatus { get; set; }

        /// <summary>
        /// Any comment that has been applied to the activity
        /// </summary>
        public string Comment { get; set; }

        /// <summary>
        /// The hanford id of the actor who completed the activity
        /// </summary>
        public string ActedUserId { get; set; }

        /// <summary>
        /// The hanford id of the actor who completed the activity
        /// </summary>
        public string ActedHanfordId { get; set; }

        /// <summary>
        /// The unique identifier for the <see cref="Actor"/> item of the person who completed the activity.
        /// </summary>
        public int ActedActorId { get; set; }

        /// <summary>
        /// Whether or not the activited was ghosted based on the activity settings.
        /// </summary>
        public bool IsGhost  { get; set; }

        /// <summary>
        /// Whether or not the activity is an ad-hoc activity.
        /// </summary>
        public bool IsAdhoc  { get; set; }

        /// <summary>
        /// A list of the Actors that can act on the activity
        /// </summary>
        /// <value>A list of <see cref="Actor"/>.</value>
        public IDictionary<int,Actor> Actors { get; set; }
    }
}
