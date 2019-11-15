namespace Pnnl.Api.Approvals.Data.Sql
{
    /// <summary>
    /// Options store for the SQL implentation of the <see cref="ActivityStoreBase"/>
    /// </summary>
    public class SqlActivityStoreOptions
    {
        /// <summary>
        /// Gets or sets the name of the connection used by the store.
        /// </summary>
        /// <value>The name of the Sql connection used by the store.</value>
        public string Connection { get; set; }
    }
}
