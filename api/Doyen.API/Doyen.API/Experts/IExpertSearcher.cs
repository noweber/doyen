using Doyen.API.Dtos.Requests;
using Doyen.API.Dtos.Responses;

namespace Doyen.API.Experts
{
    /// <summary>
    /// Interface for searching and retrieving expert information.
    /// </summary>
    public interface IExpertSearcher
    {
        /// <summary>
        /// Retrieves expert metrics based on the provided search query and time range.
        /// </summary>
        /// <param name="searchQuery">The search query.</param>
        /// <param name="timeRange">The time range.</param>
        /// <returns>The list of expert metrics.</returns>
        List<ExpertMetricsDtos> GetExpertMetricsByKeywords(SearchDto searchQuery, TimeRangeDto timeRange);

        /// <summary>
        /// Retrieves detailed information about an expert identified by the specified identifier.
        /// </summary>
        /// <param name="identifier">The identifier of the expert.</param>
        /// <returns>The expert details.</returns>
        ExpertDetailsDto GetExpertDetailsByIdentifier(string identifier);

        /// <summary>
        /// Retrieves a list of collaborators for the expert identified by the specified identifier.
        /// </summary>
        /// <param name="identifier">The identifier of the expert.</param>
        /// <returns>The list of collaborators.</returns>
        List<CollaboratorDto> GetCollaboratorsByExpertIdentifer(string identifier);
    }
}
