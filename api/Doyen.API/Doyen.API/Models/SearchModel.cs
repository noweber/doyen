using System.ComponentModel.DataAnnotations;

namespace Doyen.API.Models
{
    public class SearchModel
    {
        [Required]
        public string? Keywords { get; set; }

        [EnumDataType(typeof(SearchResultsOrdering))]
        public SearchResultsOrdering OrderBy { get; set; } = SearchResultsOrdering.Citations;

        public bool OrderDescending { get; set; } = true;
    }
}
