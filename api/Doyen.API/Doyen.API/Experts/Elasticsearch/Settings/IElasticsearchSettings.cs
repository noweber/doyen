namespace Doyen.API.Experts.Elasticsearch.Settings
{
    /// <summary>
    /// Interface for Elasticsearch settings.
    /// </summary>
    public interface IElasticsearchSettings
    {
        /// <summary>
        /// Gets the URL of the Elasticsearch server.
        /// </summary>
        public string Url { get; }

        /// <summary>
        /// Gets the username to use for authentication with the Elasticsearch server.
        /// </summary>
        public string Username { get; }

        /// <summary>
        /// Gets the password to use for authentication with the Elasticsearch server.
        /// </summary>
        public string Password { get; }

        /// <summary>
        /// Gets the maximum number of records to return in a search query.
        /// </summary>
        public int SearchRecordsLimit { get; }
    }
}
