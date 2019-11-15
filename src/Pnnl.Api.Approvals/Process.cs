using System;
using System.Collections.Generic;

namespace Pnnl.Api.Approvals
{
    /// <summary>
    /// Data class to hold the information for an instantiated approvals process.
    /// </summary>
    public class Process
    {
        /// <summary>
        /// Gets the unique identifier assigned to this resource.
        /// </summary>
        /// <value>A <see cref="int"/> that uniquely identifies this resource.</value>
        public int ProcessId { get; set; }

        /// <summary>
        /// Gets the unique identifier of the process definition that created this resource.
        /// </summary>
        /// <value>A <see cref="int"/> that uniquely identifies the process definition.</value>
        public int ProcessDefinitionId { get; set; }

        /// <summary>
        /// Gets the document type name of the resource.
        /// </summary>
        /// <value>The name of the document type.</value>
        public string DocumentTypeName { get; set; }
        
        /// <summary>
        /// The document id of the item provided by the user or the source system.
        /// </summary>
        /// <remarks>This field is not guaranteed to be unique, but integrations are encouraged
        /// to submit unique ids for their own correlation and to be able to provide better search 
        /// results.</remarks>
        /// <value>The name of the document type.</value>
        public string DocumentId { get; set; }
        
        /// <summary>
        /// This is the title of the document.
        /// </summary>
        /// <remarks>This is also know as the document description and the doc at a glance.</remarks>
        /// <value>The title of the document.</value>
        public string DocumentTitle { get; set; }
        
        /// <summary>
        /// The hanford id of the individual listed as the originator
        /// </summary>
        /// <remarks>The originator of the document should be the person who routed the document for approval.</remarks>
        /// <value>The title of the document.</value>
        public string OriginatorHanfordId { get; set; }
        
        /// <summary>
        /// The hanford id of the individual listed as the beneficiary
        /// </summary>
        /// <remarks>The beneficiary of the document should be the person who benifits from the approval. Depending on the 
        /// integration this may be the travelor, the author of the document, the person who timeform it is, etc.</remarks>
        /// <value>The title of the document.</value>
        public string BeneficiaryHanfordId { get; set; }
        
        /// <summary>
        /// The current state of the process
        /// </summary>
        /// <value>NULL|APPROVED|PEDNING|TERMINATED</value>
        public string ProcessState { get; set; }

        /// <summary>
        /// The current status of the process
        /// </summary>
        public string ProcessStatus { get; set; }

        /// <summary>
        /// The date the process was created
        /// </summary>
        public DateTime CreateDateTime { get; set; }

        /// <summary>
        /// The date the process was last updated
        /// </summary>
        public DateTime LastChangeDateTime { get; set; }

        /// <summary>
        /// The hanford id of the user who made the last update to the process
        /// </summary>
        public string LastChangeHanfordId { get; set; }

        /// <summary>
        /// A list of the activities that are included in the process
        /// </summary>
        /// <value>A list of <see cref="Activity"/>.</value>
        public IDictionary<int,Activity> Activities { get; set; }


        //  These are temporary


        /// <summary>
        /// The hanford id of the individual listed as the originator
        /// </summary>
        /// <remarks>The originator of the document should be the person who routed the document for approval.</remarks>
        /// <value>The title of the document.</value>
        public string OriginatorId { get; set; }

        /// <summary>
        /// The hanford id of the individual listed as the beneficiary
        /// </summary>
        /// <remarks>The beneficiary of the document should be the person who benifits from the approval. Depending on the 
        /// integration this may be the travelor, the author of the document, the person who timeform it is, etc.</remarks>
        /// <value>The title of the document.</value>
        public string BeneficiaryId { get; set; }
    }
}
