namespace Doyen.API.Dtos.Requests
{
    /// <summary>
    /// Represents a time range for filtering data.
    /// </summary>
    public class TimeRangeDto
    {
        /// <summary>
        /// Gets or sets the lower bound of the time range.
        /// Defaults to five years ago from the current date and time.
        /// </summary>
        public DateTime GreaterThan { get; set; } = DateTime.Now.AddYears(-5);

        /// <summary>
        /// Gets or sets the upper bound of the time range.
        /// Defaults to the current date and time.
        /// </summary>
        public DateTime LessThan { get; set; } = DateTime.Now;
    }
}
