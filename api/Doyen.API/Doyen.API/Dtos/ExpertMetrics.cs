namespace Doyen.API.Dtos
{
    public class ExpertMetrics : Expert
    {
        public ExpertMetrics(string? firstName, string? lastName, string? identifier) : base(firstName, lastName, identifier)
        {
        }

        public int PublicationsCount { get; private set; }

        public int CitationsCount { get; private set; }

        public float RelevancySum { get; private set; }

        public void IncrementPublicationsCount()
        {
            PublicationsCount++;
        }

        public void AddToCitationsCount(int value)
        {
            CitationsCount += value;
        }

        public void AddToRelevancySum(float value)
        {
            RelevancySum += value;
        }
    }
}
