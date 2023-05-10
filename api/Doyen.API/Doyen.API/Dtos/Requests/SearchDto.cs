using System.ComponentModel.DataAnnotations;

namespace Doyen.API.Dtos.Requests
{
    /// <summary>
    /// Represents a search query.
    /// </summary>
    public class SearchDto
    {
        /// <summary>
        /// Gets or sets the keywords for the search.
        /// </summary>
        [Required]
        public string? Keywords { get; set; }

        /// <summary>
        /// Gets or sets the ordering criteria for the search results.
        /// </summary>
        [EnumDataType(typeof(SearchResultsOrdering))]
        public SearchResultsOrdering OrderBy { get; set; } = SearchResultsOrdering.Citations;

        /// <summary>
        /// Gets or sets a value indicating whether the search results should be ordered in descending order.
        /// </summary>
        public bool OrderDescending { get; set; } = true;
    }
}
