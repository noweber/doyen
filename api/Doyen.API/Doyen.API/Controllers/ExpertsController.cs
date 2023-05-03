using Doyen.API.Dtos;
using Doyen.API.Experts;
using Doyen.API.Logging;
using Doyen.API.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Doyen.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public sealed class ExpertsController : ControllerBase
    {
        private readonly IExpertSearcher searcher;

        private readonly ITraceLogger logger;

        public ExpertsController(IExpertSearcher expertSearcher, ITraceLogger traceLogger)
        {
            searcher = expertSearcher ?? throw new ArgumentNullException(nameof(searcher));
            logger = traceLogger ?? throw new ArgumentNullException(nameof(traceLogger));
        }

        [HttpGet("search")]
        [ProducesDefaultResponseType]
        [ProducesResponseType(typeof(List<ExpertMetrics>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<List<ExpertMetrics>> GetExpertsSearchAsync([FromQuery] SearchModel searchQuery, [FromQuery] PaginationModel limitOffset, [FromQuery] TimeRangeModel timeRange)
        {
            try
            {
                List<ExpertMetrics> results = searcher.GetExpertMetricsByKeywords(searchQuery, timeRange);

                // Order the results based on the query parameter:
                switch (searchQuery.OrderBy)
                {
                    case SearchResultsOrdering.Relevancy:
                        results.Sort((p1, p2) => p2.RelevancySum.CompareTo(p1.RelevancySum));
                        break;
                    case SearchResultsOrdering.Citations:
                        results.Sort((p1, p2) => p2.CitationsCount.CompareTo(p1.CitationsCount));
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
        public ActionResult<ExpertDetails> GetExpertDetailsById([FromRoute] string identifier)
        {
            try
            {
                return new JsonResult(searcher.GetExpertDetailsByIdentifier(identifier),
                new JsonSerializerOptions { PropertyNamingPolicy = null }
                );
            }
            catch (Exception exception)
            {
                logger.TraceException(exception);
                return StatusCode(500);
            }
        }

        [HttpGet("{identifier}/collaborators")]
        [ProducesDefaultResponseType]
        [ProducesResponseType(typeof(List<Expert>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<List<Collaborator>> GetCollaboratorsCountForExpertById([FromRoute] string identifier)
        {
            try
            {
                Dictionary<Expert, int> collaborationCounts = new();
                HashSet<string> collaboratorsSeen = new();
                var expertDetails = searcher.GetExpertDetailsByIdentifier(identifier);
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