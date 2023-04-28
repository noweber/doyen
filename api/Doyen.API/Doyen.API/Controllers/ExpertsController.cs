using Doyen.API.Dtos;
using Doyen.API.Elasticsearch.Settings;
using Doyen.API.Logging;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Globalization;
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

        private readonly ITraceLogger logger;

        public ExpertsController(IElasticsearchSettings elasticsearchSettings, ITraceLogger traceLogger)
        {
            settings = elasticsearchSettings ?? throw new ArgumentNullException(nameof(elasticsearchSettings));
            logger = traceLogger ?? throw new ArgumentNullException(nameof(traceLogger));
        }

        [HttpGet("search")]
        [ProducesDefaultResponseType]
        [ProducesResponseType(typeof(List<ExpertMetrics>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<ExpertMetrics>>> GetExpertsSearchAsync([FromQuery] string keywords, [FromQuery] int limit = 50, [FromQuery] int offset = 0)
        {
            try
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
                var content = new StringContent("{\r\n \"size\": " + settings.SearchRecordsLimit + ",    \"query\": {\r\n        \"simple_query_string\": {\r\n            \"default_operator\": \"and\",\r\n            \"fields\": [\r\n            \"title\",\r\n            \"abstract\",\r\n            \"mesh_annotations.text\"\r\n            ],\r\n            \"query\": \"" + keywords + "\"\r\n        }\r\n    }\r\n}", null, "application/json");
                request.Content = content;
                var response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();
                var responseJson = await response.Content.ReadAsStringAsync();

                dynamic responseObject = JObject.Parse(responseJson);
                dynamic hits = responseObject.hits.hits;

                Dictionary<string, ExpertMetrics> experts = new();
                foreach (var subhit in hits)
                {
                    foreach (var author in subhit._source.authors)
                    {
                        string firstName = author.first_name;
                        string lastName = author.last_name;
                        string identifier = author.identifier;
                        string key = firstName + ":" + lastName;
                        if (!string.IsNullOrEmpty(identifier))
                        {
                            key += ":" + identifier;
                        }


                        string score = subhit._score;
                        score = score[1..^1];
                        float relevancy = float.Parse(score, CultureInfo.InvariantCulture);

                        if (experts.ContainsKey(key))
                        {
                            experts[key].AddToRelevancySum(relevancy);
                            experts[key].IncrementPublicationsCount();
                        }
                        else
                        {
                            var expert = new ExpertMetrics(firstName, lastName, identifier);
                            if (experts.TryAdd(key, expert))
                            {
                                if (subhit._score != null)
                                {
                                    experts[key].AddToRelevancySum(relevancy);
                                }

                                experts[key].IncrementPublicationsCount();
                            }
                        }
                    }

                }
                List<ExpertMetrics> results = new List<ExpertMetrics>();
                foreach (var expert in experts)
                {
                    results.Add(expert.Value);
                }

                return new JsonResult(
                    results,
                    new JsonSerializerOptions { PropertyNamingPolicy = null }
                );
            }
            catch (Exception exception)
            {
                logger.TraceException(exception);
                return StatusCode(500);
            }
        }

        [HttpGet("{identifier}")]
        [ProducesDefaultResponseType]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<ExpertDetails> GetExpertById([FromRoute] string identifier)
        {
            try
            {
                var expert = CreateRandomExpert();
                return new ActionResult<ExpertDetails>(new ExpertDetails(expert.Name, expert.Identifier.ToString(), "Institution " + Guid.NewGuid().ToString()));
            }
            catch (Exception exception)
            {
                logger.TraceException(exception);
                return StatusCode(500);
            }
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
            try
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
            catch (Exception exception)
            {
                logger.TraceException(exception);
                return StatusCode(500);
            }
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
