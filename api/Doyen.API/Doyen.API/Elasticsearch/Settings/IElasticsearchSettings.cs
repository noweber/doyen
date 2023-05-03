namespace Doyen.API.Elasticsearch.Settings
{
    public interface IElasticsearchSettings
    {
        public string Url { get; }

        public string Username { get; }

        public string Password { get; }

        public int SearchRecordsLimit { get; }
    }
}
