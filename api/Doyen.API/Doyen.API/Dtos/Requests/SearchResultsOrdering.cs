namespace Doyen.API.Dtos.Requests
{
    /// <summary>
    /// Represents the ordering criteria for search results.
    /// </summary>
    public enum SearchResultsOrdering
    {
        /// <summary>
        /// Order by relevancy.
        /// </summary>
        Relevancy = 0,

        /// <summary>
        /// Order by number of publications.
        /// </summary>
        Publications = 1,

        /// <summary>
        /// Order by number of citations.
        /// </summary>
        Citations = 2
    }
}
