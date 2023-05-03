using Doyen.API.Dtos;
using Doyen.API.Models;

namespace Doyen.API.Experts
{
    public interface IExpertSearcher
    {
        List<ExpertMetrics> GetExpertMetricsByKeywords(SearchModel searchQuery, TimeRangeModel timeRange);

        ExpertDetails GetExpertDetailsByIdentifier(string identifier);

        List<Collaborator> GetCollaboratorsByExpertIdentifer(string identifer);
    }
}
