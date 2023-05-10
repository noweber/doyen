namespace Doyen.API.Dtos.Responses
{
    public class ExpertMetricsDtos : ExpertDto
    {
        public ExpertMetricsDtos() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExpertMetricsDtos"/> class with the specified first name, last name, and identifier.
        /// </summary>
        /// <param name="firstName">The first name of the expert.</param>
        /// <param name="lastName">The last name of the expert.</param>
        /// <param name="identifier">The unique identifier of the expert.</param>
        public ExpertMetricsDtos(string? firstName, string? lastName, string? identifier) : base(firstName, lastName, identifier)
        {
        }

        /// <summary>
        /// Gets the count of publications.
        /// </summary>
        public int PublicationsCount { get; set; }

        /// <summary>
        /// Gets the count of citations.
        /// </summary>
        public int CitationsCount { get; set; }

        /// <summary>
        /// Gets the sum of relevancy values.
        /// </summary>
        public float RelevancySum { get; set; }

        /// <summary>
        /// Increments the count of publications by 1.
        /// </summary>
        public void IncrementPublicationsCount()
        {
            PublicationsCount++;
        }

        /// <summary>
        /// Adds the specified value to the count of citations.
        /// </summary>
        /// <param name="value">The value to add to the count of citations.</param>
        public void AddToCitationsCount(int value)
        {
            CitationsCount += value;
        }

        /// <summary>
        /// Adds the specified value to the sum of relevancy values.
        /// </summary>
        /// <param name="value">The value to add to the sum of relevancy values.</param>
        public void AddToRelevancySum(float value)
        {
            RelevancySum += value;
        }
    }
}
