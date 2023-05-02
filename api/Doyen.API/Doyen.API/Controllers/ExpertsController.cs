using Doyen.API.Dtos;
using Doyen.API.Elasticsearch.Settings;
using Doyen.API.Logging;
using Doyen.API.Models;
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
        public async Task<ActionResult<List<ExpertMetrics>>> GetExpertsSearchAsync([FromQuery] SearchModel searchQuery, [FromQuery] PaginationModel limitOffset, [FromQuery] TimeRangeModel timeRange)
        {
            try
            {
                // TODO: Move HTTP client code into a service class and inject it as a dependency into the controller.
                using HttpClientHandler httpClientHandler = new()
                {
                    ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                };
                using HttpClient client = new(httpClientHandler);
                string username = settings.Username;
                string password = settings.Password;
                string credentials = $"{username}:{password}";
                byte[] bytes = Encoding.UTF8.GetBytes(credentials);
                string encoded = Convert.ToBase64String(bytes);
                string authorizationHeader = $"Basic {encoded}";
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", encoded);
                using HttpRequestMessage request = new(HttpMethod.Post, settings.Url);
    
                //using StringContent content = new("{\r\n \"size\": " + settings.SearchRecordsLimit + ",    \"query\": {\r\n        \"simple_query_string\": {\r\n            \"default_operator\": \"and\",\r\n            \"fields\": [\r\n            \"title\",\r\n            \"abstract\",\r\n            \"mesh_annotations.text\"\r\n            ],\r\n            \"query\": \"" + searchQuery.Keywords + "\"\r\n        }\r\n    }\r\n}", null, "application/json");
                //using StringContent content = new("{\"size\": " + settings.SearchRecordsLimit + ",\"query\":{\"bool\":{\"must\":[{\"simple_query_string\":{\"default_operator\":\"and\",\"fields\":[\"title\",\"abstract\",\"mesh_annotations.text\"],\"query\":\"" + searchQuery.Keywords + "\"}}]}}}", null, "application/json");
                using StringContent content = new("{\"size\": " + settings.SearchRecordsLimit + ",\"query\":{\"bool\":{\"must\":[{\"simple_query_string\":{\"default_operator\":\"and\",\"fields\":[\"title\",\"abstract\",\"mesh_annotations.text\"],\"query\":\"" + searchQuery.Keywords + "\"}},{\"range\":{\"publication_date\":{\"gte\":\"" + timeRange.GreaterThan.ToString(TimeRangeModel.DATE_TIME_FORMAT) + "\",\"lte\":\"" + timeRange.LessThan.ToString(TimeRangeModel.DATE_TIME_FORMAT) + "\"}}}]}}}", null, "application/json");
                request.Content = content;
                using (var response = await client.SendAsync(request))
                {
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
                            if (!string.IsNullOrEmpty(firstName) && !string.IsNullOrEmpty(lastName))
                            {
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

                    }

                    List<ExpertMetrics> results = new();
                    foreach (var expert in experts)
                    {
                        results.Add(expert.Value);
                    }

                    // Order the results based on the query parameter:
                    switch (searchQuery.OrderBy)
                    {
                        case SearchResultsOrdering.Relevancy:
                            results.Sort((p1, p2) => p2.RelevancySum.CompareTo(p1.RelevancySum));
                            break;
                        case SearchResultsOrdering.Publications:
                        default:
                            results.Sort((p1, p2) => p2.PublicationsCount.CompareTo(p1.PublicationsCount));
                            break;
                    }
                    if (!searchQuery.OrderDescending)
                    {
                        results.Reverse();
                    }

                    // Return the paginated results:
                    int pageStartIndex = limitOffset.Offset * limitOffset.Limit;
                    int pageEndIndex = pageStartIndex + limitOffset.Limit - 1;
                    if (pageEndIndex < results.Count)
                    {
                        return new JsonResult(
                        results.GetRange(pageStartIndex, pageEndIndex - 1),
                        new JsonSerializerOptions { PropertyNamingPolicy = null }
                        );
                    }
                    else
                    {
                        return new JsonResult(
                        results,
                        new JsonSerializerOptions { PropertyNamingPolicy = null }
                    );
                    }
                }
            }
            catch (Exception exception)
            {
                logger.TraceException(exception);
                return StatusCode(500);
            }
        }



        private HashSet<string> GetAuthorKeysFromSubhitSource(dynamic source)
        {
            HashSet<string> results = new();
            foreach (var author in source.authors)
            {
                var authorKey = GetAuthorKeyFromSourceAuthor(author);
                if (authorKey != null && !results.Contains(authorKey))
                {
                    results.Add(authorKey);
                }
            }
            return results;
        }

        private string? GetAuthorKeyFromSourceAuthor(dynamic author)
        {
            string firstName = author.first_name;
            string lastName = author.last_name;
            string identifier = author.identifier;
            string? key = null;
            if (!string.IsNullOrEmpty(firstName) && !string.IsNullOrEmpty(lastName))
            {
                key = firstName + ":" + lastName;
                if (!string.IsNullOrEmpty(identifier))
                {
                    key += ":" + identifier;
                }
            }
            return key;
        }

        private List<Publication> GetPublicationsFromSubhits(dynamic subhits)
        {
            List<Publication> results = new();
            foreach (var subhit in subhits)
            {
                results.Add(GetPublicationFromSubhitsSource(subhit._source));
            }
            return results;
        }

        private Publication GetPublicationFromSubhitsSource(dynamic source)
        {
            string pubMedId = string.Empty;
            if (source.pmid != null)
            {
                pubMedId = source.pmid;//;[1..^1];
            }
            var experts = GetExpertsFromHitSourceAuthors(source.authors);
            return new Publication(pubMedId, experts);
        }

        private List<Expert> GetExpertsFromHitSourceAuthors(dynamic authors)
        {
            List<Expert> results = new();
            foreach (var author in authors)
            {
                results.Add(GetExpertFromAuthor(author));
            }
            return results;
        }

        private Expert GetExpertFromAuthor(dynamic author)
        {
            string? firstName = null;
            if (author.first_name != null)
            {
                firstName = author.first_name;
            }
            string? lastName = null;
            if (author.last_name != null)
            {
                lastName = author.last_name;
            }
            string? identifier = null;
            if (author.identifier != null)
            {
                identifier = author.identifier;
            }
            return new Expert(firstName, lastName, identifier);
        }

        private Expert GetExpertByIdFromSubhits(dynamic subhits, string identifier)
        {
            foreach (var subhit in subhits)
            {
                var authors = subhit._source.authors;
                foreach (var author in authors)
                {
                    if (author.identifier != null && author.identifier == identifier)
                    {
                        string? firstName = null;
                        if (author.first_name != null)
                        {
                            firstName = author.first_name;
                        }
                        string? lastName = null;
                        if (author.last_name != null)
                        {
                            lastName = author.last_name;
                        }
                        string? id = null;
                        if (author.identifier != null)
                        {
                            id = author.identifier;
                        }
                        return new Expert(firstName, lastName, id);
                    }
                }
            }
            return null;
        }

        [HttpGet("{identifier}")]
        [ProducesDefaultResponseType]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ExpertDetails>> GetExpertDetailsById([FromRoute] string identifier)
        {
            try
            {
                return new JsonResult(
                await GetExpertDetailsByIdentifier(identifier),
                new JsonSerializerOptions { PropertyNamingPolicy = null }
                );
            }
            catch (Exception exception)
            {
                logger.TraceException(exception);
                return StatusCode(500);
            }
        }

        private async Task<ExpertDetails> GetExpertDetailsByIdentifier(string identifier)
        {

            // TODO: Move HTTP client code into a service class and inject it as a dependency into the controller.
            using HttpClientHandler httpClientHandler = new()
            {
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            };
            using HttpClient client = new(httpClientHandler);
            string username = settings.Username;
            string password = settings.Password;
            string credentials = $"{username}:{password}";
            byte[] bytes = Encoding.UTF8.GetBytes(credentials);
            string encoded = Convert.ToBase64String(bytes);
            string authorizationHeader = $"Basic {encoded}";
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", encoded);
            using HttpRequestMessage request = new(HttpMethod.Post, settings.Url);
            using StringContent content = new("{\"query\":{\"bool\":{\"should\":[{\"term\":{\"authors.identifier.keyword\":{\"value\":\"" + identifier + "\"}}},{\"terms\":{\"authors.identifier.keyword\":[\"" + identifier + "\"]}}]}}}", null, "application/json");
            request.Content = content;
            using var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var responseJson = await response.Content.ReadAsStringAsync();
            return GetExpertDetailsByIdentifierFromElasticSearchDocumentsJson(responseJson, identifier);
        }

        private ExpertDetails GetExpertDetailsByIdentifierFromElasticSearchDocumentsJson(string documentsJson, string identifier)
        {
            dynamic responseObject = JObject.Parse(documentsJson);
            dynamic hits = responseObject.hits.hits;
            var publications = GetPublicationsFromSubhits(hits);
            var expert = GetExpertByIdFromSubhits(hits, identifier);
            return new ExpertDetails(expert.Name, identifier, publications);
        }

        [HttpGet("{identifier}/collaborators")]
        [ProducesDefaultResponseType]
        [ProducesResponseType(typeof(List<Expert>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<Collaborator>>> GetCollaboratorsCountForExpertById([FromRoute] string identifier)
        {
            try
            {
                Dictionary<Expert, int> collaborationCounts = new();
                var expertDetails = await GetExpertDetailsByIdentifier(identifier);
                if (expertDetails != null && expertDetails.Publications != null)
                {
                    foreach (var publication in expertDetails.Publications)
                    {
                        if (publication.Authors != null)
                        {
                            foreach (var author in publication.Authors)
                            {
                                if (author != null)
                                {
                                    if (collaborationCounts.ContainsKey(author))
                                    {
                                        collaborationCounts[author] += 1;
                                    }
                                    else
                                    {
                                        collaborationCounts.TryAdd(author, 1);
                                    }
                                }
                            }
                        }
                    }
                }
                List<Collaborator> collaborators = new List<Collaborator>();
                foreach (var entry in collaborationCounts)
                {
                    collaborators.Add(new Collaborator(entry.Key, entry.Value));
                }
                collaborators.Sort((p1, p2) => p2.NumberOfCollaborations.CompareTo(p1.NumberOfCollaborations));
                return new JsonResult(
                collaborators,
                new JsonSerializerOptions { PropertyNamingPolicy = null }
                );
            }
            catch (Exception exception)
            {
                logger.TraceException(exception);
                return StatusCode(500);
            }
        }
    }
}
