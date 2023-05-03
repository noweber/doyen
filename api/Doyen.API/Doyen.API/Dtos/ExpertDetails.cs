namespace Doyen.API.Dtos
{
    public class ExpertDetails : Expert
    {
        public ExpertDetails(string firstName, string lastName, string? identifier, List<Publication>? publications = null) : base(firstName, lastName, identifier)
        {
            if (publications != null)
            {
                Publications = publications;
            }
        }
        public ExpertDetails(string name, string? identifier, List<Publication>? publications = null) : base(name, identifier)
        {
            if (publications != null)
            {
                Publications = publications;
            }
        }

        public List<Publication>? Publications { get; set; }
    }
}
