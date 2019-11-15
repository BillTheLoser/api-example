namespace Pnnl.Api.Approvals.Data.Http
{
    /// <summary>
    /// Represents the options used to configure an <see cref="HttpPersonStore"/>.
    /// </summary>
    public class HttpPersonStoreOptions
    {
        /// <summary>
        /// Gets or sets the name of the HTTP client used by the service.
        /// </summary>
        /// <value>The name of the HTTP client used by the service.</value>
        public string Client { get; set; }
    }
}
