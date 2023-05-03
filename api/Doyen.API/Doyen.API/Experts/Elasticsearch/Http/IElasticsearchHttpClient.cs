namespace Doyen.API.Experts.Elasticsearch.Http
{
    public interface IElasticsearchHttpClient
    {
        Task<string> SendSearchPostRequest(string requestBody);
    }
}
