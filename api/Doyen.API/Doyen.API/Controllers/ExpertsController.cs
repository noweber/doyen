using Doyen.API.Dtos.Requests;
using Doyen.API.Dtos.Responses;
using Doyen.API.Experts;
using Doyen.API.Logging;
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
            // Assign the expert searcher and trace logger dependencies
            searcher = expertSearcher ?? throw new ArgumentNullException(nameof(searcher));
            logger = traceLogger ?? throw new ArgumentNullException(nameof(traceLogger));
        }

        /// <summary>
        /// Retrieves a list of experts based on the search criteria.
        /// </summary>
        /// <param name="searchQuery">The search criteria.</param>
        /// <param name="limitOffset">Pagination parameters.</param>
        /// <param name="timeRange">Time range parameters.</param>
        [HttpGet("search")]
        [ProducesDefaultResponseType]
        [ProducesResponseType(typeof(List<ExpertMetricsDtos>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<List<ExpertMetricsDtos>> GetExpertsSearchAsync([FromQuery] SearchDto searchQuery, [FromQuery] PaginationDto limitOffset, [FromQuery] TimeRangeDto timeRange)
        {
            try
            {
                List<ExpertMetricsDtos> results = searcher.GetExpertMetricsByKeywords(searchQuery, timeRange);

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
                // Log any exceptions that occur
                logger.TraceException(exception);
                return StatusCode(500);
            }
        }


        /// <summary>
        /// Retrieves details of an expert by their identifier.
        /// </summary>
        /// <param name="identifier">The identifier of the expert.</param>
        [HttpGet("{identifier}")]
        [ProducesDefaultResponseType]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<ExpertDetailsDto> GetExpertDetailsById([FromRoute] string identifier)
        {
            try
            {
                // Retrieve expert details by the provided identifier
                ExpertDetailsDto expertDetails = searcher.GetExpertDetailsByIdentifier(identifier);

                return new JsonResult(expertDetails,
                    new JsonSerializerOptions { PropertyNamingPolicy = null }
                );
            }
            catch (Exception exception)
            {
                // Log any exceptions that occur
                logger.TraceException(exception);
                return StatusCode(500);
            }
        }

        /// <summary>
        /// Retrieves the count of collaborators for an expert based on their identifier.
        /// </summary>
        /// <param name="identifier">The identifier of the expert.</param>
        [HttpGet("{identifier}/collaborators")]
        [ProducesDefaultResponseType]
        [ProducesResponseType(typeof(List<ExpertDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<List<CollaboratorDto>> GetCollaboratorsCountForExpertById([FromRoute] string identifier)
        {
            try
            {
                List<CollaboratorDto> collaborators = searcher.GetCollaboratorsByExpertIdentifer(identifier);
                collaborators.Sort((p1, p2) => p2.NumberOfCollaborations.CompareTo(p1.NumberOfCollaborations));

                return new JsonResult(
                    collaborators,
                    new JsonSerializerOptions { PropertyNamingPolicy = null }
                );
            }
            catch (Exception exception)
            {
                // Log any exceptions that occur
                logger.TraceException(exception);
                return StatusCode(500);
            }
        }
    }
}
