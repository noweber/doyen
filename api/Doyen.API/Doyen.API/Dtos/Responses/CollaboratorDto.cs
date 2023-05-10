namespace Doyen.API.Dtos.Responses
{
    /// <summary>
    /// Represents a collaborator, which is a specialized type of expert.
    /// It includes the number of collaborations the collaborator has.
    /// </summary>
    public class CollaboratorDto : ExpertDto
    {
        public CollaboratorDto() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CollaboratorDto"/> class with the specified parameters.
        /// </summary>
        /// <param name="firstName">The first name of the collaborator.</param>
        /// <param name="lastName">The last name of the collaborator.</param>
        /// <param name="identifier">The identifier of the collaborator.</param>
        /// <param name="collaborationCount">The number of collaborations the collaborator has.</param>
        public CollaboratorDto(string firstName, string lastName, string? identifier, int collaborationCount)
            : base(firstName, lastName, identifier)
        {
            NumberOfCollaborations = collaborationCount;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CollaboratorDto"/> class with the specified parameters.
        /// </summary>
        /// <param name="name">The name of the collaborator.</param>
        /// <param name="identifier">The identifier of the collaborator.</param>
        /// <param name="collaborationCount">The number of collaborations the collaborator has.</param>
        public CollaboratorDto(string name, string? identifier, int collaborationCount)
            : base(name, identifier)
        {
            NumberOfCollaborations = collaborationCount;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CollaboratorDto"/> class with the specified parameters.
        /// </summary>
        /// <param name="expert">The expert to create the collaborator from.</param>
        /// <param name="collaborationCount">The number of collaborations the collaborator has.</param>
        public CollaboratorDto(ExpertDto expert, int collaborationCount)
            : base(expert.Name, expert.Identifier)
        {
            NumberOfCollaborations = collaborationCount;
        }

        /// <summary>
        /// Gets or sets the number of collaborations the collaborator has.
        /// </summary>
        public int NumberOfCollaborations { get; set; }
    }
}
