using Doyen.API.Dtos;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Text.Json;

namespace Doyen.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExpertsController : ControllerBase
    {
        [HttpGet("search")]
        [ProducesDefaultResponseType]
        [ProducesResponseType(typeof(List<Expert>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<Expert>>> GetExpertsSearchAsync([FromQuery] string keywords, [FromQuery] int limit = 50, [FromQuery] int offset = 0)
        {
            if (!AreLimitAndOffsetValid(limit, offset))
            {
                return BadRequest();
            }
            // Success:
            var httpClientHandler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            };
            var client = new HttpClient(httpClientHandler);

            var request = new HttpRequestMessage(HttpMethod.Post, "https://localhost:9200/pubmed-paper-index/_search?typed_keys=true");
            request.Headers.Add("Authorization", "Basic ZWxhc3RpYzpfUXFacEJBNEMyUmRQSUVoVytndA==");

            var content = new StringContent("{\r\n    \"query\": {\r\n        \"simple_query_string\": {\r\n            \"default_operator\": \"and\",\r\n            \"fields\": [\r\n            \"title\",\r\n            \"abstract\",\r\n            \"mesh_annotations.text\"\r\n            ],\r\n            \"query\": \"" + keywords + "\"\r\n        }\r\n    }\r\n}", null, "application/json");
            request.Content = content;

            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var responseJson = await response.Content.ReadAsStringAsync();
            //dynamic responseObject = JsonSerializer.Deserialize<dynamic>(responseJson);
            List<Expert> experts = new List<Expert>();
            dynamic responseObject = JObject.Parse(responseJson);
            dynamic hits = responseObject.hits.hits;
            foreach (var subhit in hits)
            {
                foreach (var author in subhit._source.authors)
                {
                    string firstName = author.first_name;
                    string lastName = author.last_name;
                    string identifier = author.identifier;
                    var expert = new Expert(firstName, lastName, identifier);
                    experts.Add(expert);
                }
            }

            return new JsonResult(
                experts,
                new JsonSerializerOptions { PropertyNamingPolicy = null }
            );


            var elasticsearchUrl = "http://localhost:9200";
            var username = "elastic";
            var password = "_QqZpBA4C2RdPIEhW+gt";
            var indexName = "pubmed-paper-index";

            /*
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, "https://localhost:9200/pubmed-paper-index/_search?typed_keys=true");
            request.Headers.Add("Authorization", "Basic ZWxhc3RpYzpfUXFacEJBNEMyUmRQSUVoVytndA==");
            var content = new StringContent("{\r\n    \"query\": {\r\n        \"simple_query_string\": {\r\n            \"default_operator\": \"and\",\r\n            \"fields\": [\r\n            \"title\",\r\n            \"abstract\",\r\n            \"mesh_annotations.text\"\r\n            ],\r\n            \"query\": \"Covid cough classification by audio\"\r\n        }\r\n    }\r\n}", null, "application/json");
            request.Content = content;
            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            Console.WriteLine(await response.Content.ReadAsStringAsync());
            return new JsonResult(
            response,
            new JsonSerializerOptions { PropertyNamingPolicy = null });*/

            // Failed Attempt:
            /*var queryJson = $"{{\r\n  \"query\": {{\r\n\t\"simple_query_string\": {{\r\n\t  \"default_operator\": \"and\",\r\n\t\t\"fields\": [\r\n\t\t  \"title\",\r\n\t\t  \"abstract\",\r\n\t\t  \"mesh_annotations.text\"\r\n\t\t],\r\n\t\t  \"query\": \"{keywords}\"\r\n\t}}\r\n  }}\r\n}}";
            var response = await ElasticsearchQuery.Search(elasticsearchUrl, indexName, queryJson, username, password);
            return new JsonResult(
            response,
            new JsonSerializerOptions { PropertyNamingPolicy = null });*/

            // Failed Attempt:
            /*
            var pool = new SingleNodeConnectionPool(new Uri(elasticsearchUrl));
            var settings = new ConnectionSettings(pool)
            .DefaultIndex(indexName)
            .CertificateFingerprint("50:C3:A1:B5:3D:DB:15:AA:16:3F:B5:5C:61:B2:5B:41:DC:56:FC:D4:71:67:3D:FB:6E:A8:DF:A6:32:71:45:6E")
            .BasicAuthentication("elastic", "_QqZpBA4C2RdPIEhW+gt")
            .ServerCertificateValidationCallback((sender, certificate, chain, sslPolicyErrors) => true)
            .EnableApiVersioningHeader();

            var client = new ElasticClient(settings);

            // Query DSL
            var queryDSL = @"
            {
              ""query"": {
                ""simple_query_string"": {
                  ""default_operator"": ""and"",
                  ""fields"": [
                    ""title"",
                    ""abstract"",
                    ""mesh_annotations.text""
                  ],
                  ""query"": ""Covid cough classification by audio""
                }
              }
            }";

            // Elasticsearch search request
            var searchRequest = new SearchRequest<PubmedIndexDocument>(indexName)
            {
                Query = JsonConvert.DeserializeObject<QueryContainer>(queryDSL)
            };

            // Elasticsearch search response
            var searchResponse = client.Search<PubmedIndexDocument>(searchRequest);
            
            var response = searchResponse.Documents;
            return new JsonResult(
            response,
            new JsonSerializerOptions { PropertyNamingPolicy = null });*/
        }

        [HttpGet("{identifier}")]
        [ProducesDefaultResponseType]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<ExpertDetails> GetExpertById([FromRoute] string identifier)
        {
            var expert = CreateRandomExpert();
            return new ActionResult<ExpertDetails>(new ExpertDetails(expert.Name, expert.Identifier.ToString(), "Institution " + Guid.NewGuid().ToString()));
        }

        // TODO: Pagination
        [HttpGet("{identifier}/collaborators")]
        [ProducesDefaultResponseType]
        [ProducesResponseType(typeof(List<Expert>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<List<Expert>> GetCollaboratorsForExpertById([FromRoute] string identifier, [FromQuery] int limit = 50, [FromQuery] int offset = 0)
        {
            if (!AreLimitAndOffsetValid(limit, offset))
            {
                return BadRequest();
            }

            var results = new List<Expert>();

            for (int i = 0; i < 4; i++)
            {
                results.Add(CreateRandomExpert());
            }

            return new ActionResult<List<Expert>>(results);
        }

        private List<Expert> GetCovidExperts()
        {
            List<Expert> results = new List<Expert>()
            {
                new Expert("Conor", "Wall", "0000-0001-6674-692X"),
                new Expert("Li", "Zhang", "0000-0003-4263-7168"),
                new Expert("Yonghong", "Yu", null),
                new Expert("Akshi", "Kumar", null),
                new Expert("Rong", "Gao", null),

                new Expert("Santosh", "Kumar", null),
                new Expert("Mithilesh Kumar", "Chaube", null),
                new Expert("Saeed Hamood", "Alsamhi", null),
                new Expert("Sachin Kumar", "Gupta", null),
                new Expert("Mohsen", "Guizani", null),
                new Expert("Raffaele", "Gravina", null),
                new Expert("Giancarlo", "Fortino", null),

            };
            return results;
        }

        private Expert CreateRandomExpert()
        {
            return new Expert("Expert", Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
        }


        private bool AreLimitAndOffsetValid(int limit, int offset)
        {
            if (limit <= 0 || offset < 0)
            {
                return false;
            }
            return true;
        }
    }
}
