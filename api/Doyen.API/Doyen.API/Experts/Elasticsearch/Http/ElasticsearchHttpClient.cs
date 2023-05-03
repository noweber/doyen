using Doyen.API.Experts.Elasticsearch.Settings;
using System.Net.Http.Headers;
using System.Text;

namespace Doyen.API.Experts.Elasticsearch.Http
{
    public class ElasticsearchHttpClient : IElasticsearchHttpClient
    {
        private readonly IElasticsearchSettings settings;

        public ElasticsearchHttpClient(IElasticsearchSettings elasticsearchSettings)
        {
            settings = elasticsearchSettings ?? throw new ArgumentNullException(nameof(elasticsearchSettings));
        }

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
