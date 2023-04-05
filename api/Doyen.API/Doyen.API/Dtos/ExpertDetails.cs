namespace Doyen.API.Dtos
{
    public class ExpertDetails : Expert
    {
        public ExpertDetails(string firstName, string lastName, string? identifier, string? institution) : base(firstName, lastName, identifier)
        {
            LastKnownInstitution = institution;
        }
        public ExpertDetails(string name, string? identifier, string? institution) : base(name, identifier)
        {
            LastKnownInstitution = institution;
        }

        public string? LastKnownInstitution { get; set; }
    }
}
