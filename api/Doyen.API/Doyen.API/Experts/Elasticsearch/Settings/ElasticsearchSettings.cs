namespace Doyen.API.Experts.Elasticsearch.Settings
{
    /// <summary>
    /// Represents the Elasticsearch settings.
    /// </summary>
    public class ElasticsearchSettings : IElasticsearchSettings
    {
        /// <inheritdoc />
        public string Url { get; set; }

        /// <inheritdoc />
        public string Username { get; set; }

        /// <inheritdoc />
        public string Password { get; set; }

        /// <inheritdoc />
        public int SearchRecordsLimit { get; set; }
    }
}
