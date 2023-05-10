namespace Doyen.API.Dtos.Responses
{
    /// <summary>
    /// Represents the details of an expert, including their publications.
    /// </summary>
    public class ExpertDetailsDto : ExpertDto
    {
        public ExpertDetailsDto() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExpertDetailsDto"/> class with the specified parameters.
        /// </summary>
        /// <param name="firstName">The first name of the expert.</param>
        /// <param name="lastName">The last name of the expert.</param>
        /// <param name="identifier">The identifier of the expert.</param>
        /// <param name="publications">The list of publications associated with the expert (optional).</param>
        public ExpertDetailsDto(string firstName, string lastName, string? identifier, List<PublicationDto>? publications = null)
            : base(firstName, lastName, identifier)
        {
            if (publications != null)
            {
                Publications = publications;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExpertDetailsDto"/> class with the specified parameters.
        /// </summary>
        /// <param name="name">The name of the expert.</param>
        /// <param name="identifier">The identifier of the expert.</param>
        /// <param name="publications">The list of publications associated with the expert (optional).</param>
        public ExpertDetailsDto(string name, string? identifier, List<PublicationDto>? publications = null)
            : base(name, identifier)
        {
            if (publications != null)
            {
                Publications = publications;
            }
        }

        /// <summary>
        /// Gets or sets the list of publications associated with the expert.
        /// </summary>
        public List<PublicationDto>? Publications { get; set; }
    }
}
