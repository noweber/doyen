namespace Doyen.API.Experts.Elasticsearch.Http
{
    /// <summary>
    /// Represents an interface for making HTTP requests to Elasticsearch.
    /// </summary>
    public interface IElasticsearchHttpClient
    {
        /// <summary>
        /// Sends a POST request to Elasticsearch for search operation.
        /// </summary>
        /// <param name="requestBody">The request body.</param>
        /// <returns>The response from Elasticsearch as a string.</returns>
        Task<string> SendSearchPostRequest(string requestBody);
    }
}
