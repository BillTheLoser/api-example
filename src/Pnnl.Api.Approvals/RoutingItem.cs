using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Pnnl.Api.Approvals
{
    public class RoutingItem
    {
        /// <summary>
        /// The document type name of the resource.
        /// </summary>
        /// <value>The name of the document type.</value>
        [Required(AllowEmptyStrings = false, ErrorMessage ="{0} is required!")]
        public string DocumentTypeName { get; set; }
        
        /// <summary>
        /// The item id of the application that sourced the routing.  This is currently the IRI item id.
        /// </summary>
        /// <value>The item id of the source application.</value>
        [Required( ErrorMessage ="{0} is required!")]
        public int? ApplicationItemId { get; set; }
        
        /// <summary>
        /// The document id of the item provided by the user or the source system.
        /// </summary>
        /// <remarks>This field is not guaranteed to be unique, but integrations are encouraged
        /// to submit unique ids for their own correlation and to be able to provide better search 
        /// results.</remarks>
        /// <value>The name of the document type.</value>
        [Required(AllowEmptyStrings = false, ErrorMessage ="{0} is required!")]
        public string DocumentId { get; set; }
        
        /// <summary>
        /// This is the title of the document.
        /// </summary>
        /// <remarks>This is also know as the document description and the doc at a glance.</remarks>
        /// <value>The title of the document.</value>
        [Required(AllowEmptyStrings = false, ErrorMessage ="{0} is required!")]
        public string DocumentTitle { get; set; }
        
        /// <summary>
        /// The link to the document in the source system, if the document is an editable document.
        /// </summary>
        /// <value>The url to access the document.</value>
        public string DocumentEditUrl { get; set; }
        
        /// <summary>
        /// The hanford id of the individual listed as the originator
        /// </summary>
        /// <remarks>The originator of the document should be the person who routed the document for approval.</remarks>
        /// <value>The title of the document.</value>
        [Required(AllowEmptyStrings = false, ErrorMessage = "{0} is required!")]
        [StringLength(7, ErrorMessage ="A Hanford Id must be 7 characters!")]
        public string OriginatorHanfordId { get; set; }
        
        /// <summary>
        /// The hanford id of the individual listed as the beneficiary
        /// </summary>
        /// <remarks>The beneficiary of the document should be the person who benifits from the approval. Depending on the 
        /// integration this may be the travelor, the author of the document, the person who timeform it is, etc.</remarks>
        /// <value>The title of the document.</value>
        [Required(AllowEmptyStrings = false, ErrorMessage = "{0} is required!")]
        [StringLength(7, ErrorMessage ="A Hanford Id must be 7 characters!")]
        public string BeneficiaryHanfordId { get; set; }
        
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
        [Required(AllowEmptyStrings = false, ErrorMessage = "{0} is required!")]
        [StringLength(7, ErrorMessage ="A Hanford Id must be 7 characters!")]
        public string SubmitUserHanfordId { get; set; }

        /// <summary>
        /// The name of the container for records management, currently HPRM
        /// </summary>
        public string RecordsContainer { get; set; }

        /// <summary>
        /// The name of the records classification, currently HPRM
        /// </summary>
        public string RecordsClassification { get; set; }

        /// <summary>
        /// The document being routed for Approval
        /// </summary>
        /// <value>A <see cref="RoutingDocument"/>.</value>
        public RoutingDocument Document { get; set; }

        /// <summary>
        /// A list of data used to generate the routing that is in string form
        /// </summary>
        /// <value>A list of <see cref="string"/>.</value>
        public IDictionary<string,string> StringFields { get; set; }
        
        /// <summary>
        /// A list of data used to generate the routing that is in integer form
        /// </summary>
        /// <value>A list of <see cref="int"/>.</value>
        public IDictionary<string,int> IntFields { get; set; }
        
        /// <summary>
        /// A list of data used to generate the routing for replicated activities
        /// </summary>
        /// <value>A list of <see cref="ReplicatedListData"/>.</value>
        public IDictionary<string,ReplicatedListData> ListFields { get; set; }
    }
}
