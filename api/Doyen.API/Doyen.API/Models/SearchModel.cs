using Newtonsoft.Json.Converters;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Doyen.API.Models
{
    public class SearchModel
    {
        [Required]
        public string? Keywords { get; set; }

        [EnumDataType(typeof(SearchResultsOrdering))]
        public SearchResultsOrdering OrderBy { get; set; } = SearchResultsOrdering.Relevancy;
    }
}
