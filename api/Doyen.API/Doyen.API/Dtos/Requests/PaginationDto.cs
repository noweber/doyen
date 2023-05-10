using System.ComponentModel.DataAnnotations;

namespace Doyen.API.Dtos.Requests
{
    /// <summary>
    /// Represents pagination information for a request.
    /// </summary>
    public class PaginationDto
    {
        private const int MIN_LIMIT_OFFSET = 0;
        private const int MAX_LIMIT_OFFSET = 10000;

        /// <summary>
        /// Gets or sets the maximum number of items to return in a single page.
        /// </summary>
        [Range(MIN_LIMIT_OFFSET, MAX_LIMIT_OFFSET)]
        public int Limit { get; set; } = MAX_LIMIT_OFFSET;

        /// <summary>
        /// Gets or sets the offset or starting index of the items to return.
        /// </summary>
        [Range(MIN_LIMIT_OFFSET, MAX_LIMIT_OFFSET)]
        public int Offset { get; set; } = MIN_LIMIT_OFFSET;
    }
}
