namespace Doyen.API.Dtos
{
    public class Collaborator : Expert
    {
        public Collaborator(string firstName, string lastName, string? identifier, int collaborationCount) : base(firstName, lastName, identifier)
        {
            NumberOfCollaborations = collaborationCount;
        }

        public Collaborator(string name, string? identifier, int collaborationCount) : base(name, identifier)
        {
            NumberOfCollaborations = collaborationCount;
        }

        public Collaborator(Expert expert, int collaborationCount) : base(expert.Name, expert.Identifier)
        {
            NumberOfCollaborations = collaborationCount;
        }

        public int NumberOfCollaborations { get; set; }
    }
}
