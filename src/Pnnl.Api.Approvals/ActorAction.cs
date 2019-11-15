namespace Pnnl.Api.Approvals
{
    /// <summary>
    /// Data class to hold the information necessary to apply an <see cref="Actor"/> action on an <see cref="Activity"/>
    /// </summary>
    /// 
    //	<action>
    //	<processId>string</processId>
    //	<activityId>string</activityId>
    //	<comments>string</comments>
    //	<actionTaken>string</actionTaken>
    //	<websignRedirect>string</websignRedirect>
    //	<documentDescription>string</documentDescription>
    //	<actionUser>
    //		<domain>string</domain>
    //		<networkId>string</networkId>
    //	</actionUser>
    //	<document>
    //		<fileExtension>string</fileExtension>
    //		<mimeType>string</mimeType>
    //		<content>base64Binary</content>
    //		<asciiContent>string</asciiContent>
    //		<xslStylesheet>string</xslStylesheet>
    //	</document>
    //</action>
    //

    public class ActorAction
    {
        /// <summary>
        /// Gets the unique identifier assigned to this resource.
        /// </summary>
        /// <value>A <see cref="int"/> that uniquely identifies this resource.</value>
        public int ProcessId { get; set; }

        /// <summary>
        /// The unique identifier for this activity within the approvals system.
        /// </summary>
        public int ActivityId { get; set; }

        /// <summary>
        /// Any comment that has been applied to the activity
        /// </summary>
        public string Comment { get; set; }

        /// <summary>
        /// This is the title of the document.
        /// </summary>
        /// <remarks>This is also know as the document description and the doc at a glance.</remarks>
        /// <value>The title of the document.</value>
        public string DocumentTitle { get; set; }

        /// <summary>
        /// The Hanford Id of the person performing the action
        /// </summary>
        public string ActorHanfordId { get; set; }

        /// <summary>
        /// The action that is going to be applied to the activity.
        /// </summary>
        public ActorActionTaken ActionTaken { get; set; }

        // The current implementaiton of sendAction allows the ability to replace the document, I don't belive 
        // the we really need this.  We will need to send to the node to the existing web service, but I don't think that 
        // we need to allow it until someone really makes an argument.
        private RoutingDocument Document { get; set; }
    }
}
