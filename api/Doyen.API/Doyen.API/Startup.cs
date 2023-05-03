using Doyen.API.Experts.Elasticsearch.Settings;

namespace Doyen.API
{
    public static class Startup
    {
        public static IElasticsearchSettings GetElasticsearchSettings(WebApplicationBuilder builder)
        {
            return builder.Configuration.GetSection(nameof(ElasticsearchSettings)).Get<ElasticsearchSettings>();
        }
    }
}
