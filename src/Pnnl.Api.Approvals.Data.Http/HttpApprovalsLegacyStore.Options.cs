namespace Pnnl.Api.Approvals.Data.Http
{
    /// <summary>
    /// Represents the options used to configure an <see cref="HttpRouteItemStoreOptions"/>.
    /// </summary>
    public class HttpRouteItemStoreOptions: HttpClientOptions
    {
        /// <summary>
        /// Gets or sets the name of the HTTP client used by the service.
        /// </summary>
        /// <value>The name of the HTTP client used by the service.</value>
        public string Client { get; set; }

        public HttpRouteItemStoreOptions() : base() { }

    }
}
