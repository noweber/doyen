using System.Text.Json.Serialization;

namespace Doyen.API.Elasticsearch.QueryDsl
{
    public class Query
    {
        [JsonConstructor]
        public Query(
            SimpleQueryString simpleQueryString
        )
        {
            this.SimpleQueryString = simpleQueryString;
        }

        [JsonPropertyName("simple_query_string")]
        public SimpleQueryString SimpleQueryString { get; }
    }

    public class QueryDsl
    {
        [JsonConstructor]
        public QueryDsl(
            Query query
        )
        {
            this.Query = query;
        }

        [JsonPropertyName("query")]
        public Query Query { get; }
    }

    public class SimpleQueryString
    {
        [JsonConstructor]
        public SimpleQueryString(
            string defaultOperator,
            List<string> fields,
            string query
        )
        {
            this.DefaultOperator = defaultOperator;
            this.Fields = fields;
            this.Query = query;
        }

        [JsonPropertyName("default_operator")]
        public string DefaultOperator { get; }

        [JsonPropertyName("fields")]
        public IReadOnlyList<string> Fields { get; }

        [JsonPropertyName("query")]
        public string Query { get; }
    }


}
