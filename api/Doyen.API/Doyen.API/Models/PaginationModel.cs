using System.ComponentModel.DataAnnotations;

namespace Doyen.API.Models
{
    public class PaginationModel
    {
        private const int MIN_LIMIT_OFFSET = 0;

        private const int MAX_LIMIT_OFFSET = 10000;

        [Range(MIN_LIMIT_OFFSET, MAX_LIMIT_OFFSET)]
        public int Limit { get; set; } = MAX_LIMIT_OFFSET;

        [Range(MIN_LIMIT_OFFSET, MAX_LIMIT_OFFSET)]
        public int Offset { get; set; } = MIN_LIMIT_OFFSET;
    }
}
