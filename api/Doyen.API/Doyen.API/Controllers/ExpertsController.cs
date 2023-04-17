using Doyen.API.Dtos;
using Doyen.API.Elasticsearch.Settings;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Doyen.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public sealed class ExpertsController : ControllerBase
    {
        private readonly IElasticsearchSettings settings;

        public ExpertsController(IElasticsearchSettings elasticsearchSettings)
        {
            settings = elasticsearchSettings;
        }

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
            

            // TODO: Move HTTP client code into a service class and inject it as a dependency into the controller.
            var httpClientHandler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            };
            var client = new HttpClient(httpClientHandler);
            string username = settings.Username;
            string password = settings.Password;
            string credentials = $"{username}:{password}";
            byte[] bytes = Encoding.UTF8.GetBytes(credentials);
            string encoded = Convert.ToBase64String(bytes);
            string authorizationHeader = $"Basic {encoded}";
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", encoded);
            var request = new HttpRequestMessage(HttpMethod.Post, settings.Url);
            var content = new StringContent("{\r\n    \"query\": {\r\n        \"simple_query_string\": {\r\n            \"default_operator\": \"and\",\r\n            \"fields\": [\r\n            \"title\",\r\n            \"abstract\",\r\n            \"mesh_annotations.text\"\r\n            ],\r\n            \"query\": \"" + keywords + "\"\r\n        }\r\n    }\r\n}", null, "application/json");
            request.Content = content;
            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var responseJson = await response.Content.ReadAsStringAsync();

            // TODO: Create a strongly type POCO to match the Elasticsearch index document.
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
