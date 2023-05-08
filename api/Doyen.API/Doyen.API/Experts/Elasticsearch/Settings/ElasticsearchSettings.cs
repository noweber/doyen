namespace Doyen.API.Experts.Elasticsearch.Settings
{
    public class ElasticsearchSettings : IElasticsearchSettings
    {
        public string Url { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        public int SearchRecordsLimit { get; set; }
    }
}
