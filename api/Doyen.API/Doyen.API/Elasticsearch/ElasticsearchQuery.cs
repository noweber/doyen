using System.Net;
using System.Text;

namespace ElasticsearchQueries
{
    public static class ElasticsearchQuery
    {
        public static async Task<string> Search(string elasticsearchUrl, string indexName, string query, string username, string password)
        {
            try
            {
                var httpClientHandler = new HttpClientHandler
                {
                    Credentials = new NetworkCredential(username, password)
                };
                var httpClient = new HttpClient(httpClientHandler);
                var uri = new Uri($"{elasticsearchUrl}/{indexName}/_search?typed_keys=true");

                var content = new StringContent(query, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync(uri, content);
                //response.EnsureSuccessStatusCode();

                var responseBody = await response.Content.ReadAsStringAsync();

                return responseBody;
            }
            catch (Exception ex)
            {
                return $"An error occurred: {ex.Message}";
            }
        }
    }

}
