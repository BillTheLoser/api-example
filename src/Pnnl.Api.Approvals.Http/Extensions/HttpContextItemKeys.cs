namespace Pnnl.Api.Approvals.Http.Extensions
{
    /// <summary>
    /// Provides the keys used to identify shared data within the scope of an HTTP request.
    /// </summary>
    public static class HttpContextItemKeys
    {
        /// <summary>
        /// Provides the keys used to identify shared aggregate user data within the scope of an HTTP request.
        /// </summary>
        public static class ProcessApi
        {
            private const string user = "system-user";

            /// <summary>
            /// The encapsulated user key used to identify the logged in user from the HTTP request.
            /// </summary>
            public static string User => user;
        }
    }
}
