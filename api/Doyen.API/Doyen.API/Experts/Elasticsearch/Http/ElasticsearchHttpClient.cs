using Doyen.API.Experts.Elasticsearch.Settings;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;
using System.Text;

namespace Doyen.API.Experts.Elasticsearch.Http
{
    [ExcludeFromCodeCoverage]
    /// <summary>
    /// Represents an HTTP client for making requests to Elasticsearch.
    /// </summary>
    public class ElasticsearchHttpClient : IElasticsearchHttpClient
    {
        private readonly IElasticsearchSettings settings;

        /// <summary>
        /// Initializes a new instance of the ElasticsearchHttpClient class.
        /// </summary>
        /// <param name="elasticsearchSettings">The Elasticsearch settings.</param>
        /// <exception cref="ArgumentNullException">Thrown when elasticsearchSettings is null.</exception>
        public ElasticsearchHttpClient(IElasticsearchSettings elasticsearchSettings)
        {
            settings = elasticsearchSettings ?? throw new ArgumentNullException(nameof(elasticsearchSettings));
        }

        /// <inheritdoc />
        public async Task<string> SendSearchPostRequest(string requestBody)
        {
            using HttpClientHandler httpClientHandler = new()
            {
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            };
            using HttpClient client = new(httpClientHandler);
            string credentials = $"{settings.Username}:{settings.Password}";
            byte[] bytes = Encoding.UTF8.GetBytes(credentials);
            string encoded = Convert.ToBase64String(bytes);
            string authorizationHeader = $"Basic {encoded}";
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", encoded);
            using HttpRequestMessage request = new(HttpMethod.Post, settings.Url);
            using StringContent content = new(requestBody, null, "application/json");
            request.Content = content;
            using var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }
    }
}
