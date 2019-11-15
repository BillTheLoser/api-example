using System;

namespace Pnnl.Api.Approvals
{
    /// <summary>
    /// The information about document being routed for approval
    /// </summary>
    public class RoutingDocument
    {
        /// <summary>
        /// The file extension for the document
        /// </summary>
        public string FileExtension { get; set; }

        /// <summary>
        /// The mimetype for the doucment so we know hwo to display it in the web
        /// </summary>
        public string MimeType { get; set; }

        /// <summary>
        /// The binary contents of the document
        /// </summary>
        public Byte[] Content { get; set; }

        /// <summary>
        /// An optional style sheet if the document submitted is an xml document
        /// </summary>
        public string XslStyleSheet { get; set; }

        /// <summary>
        /// Optional Ascii Content if the document is not a binary document.
        /// </summary>
        public string AsciiContent { get; set; }
    }
}
