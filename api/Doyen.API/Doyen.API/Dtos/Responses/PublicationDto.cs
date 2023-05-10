namespace Doyen.API.Dtos.Responses
{
    public class PublicationDto
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PublicationDto"/> class with the specified PubMed ID, authors, publication date, and referenced PubMed IDs.
        /// </summary>
        /// <param name="pubMedId">The PubMed ID of the publication.</param>
        /// <param name="authors">The list of authors associated with the publication.</param>
        /// <param name="publicationDate">The publication date of the publication.</param>
        /// <param name="referencedPubMedIds">The list of PubMed IDs referenced by the publication.</param>
        public PublicationDto(string? pubMedId = null, List<ExpertDto>? authors = null, string? publicationDate = null, List<string>? referencedPubMedIds = null)
        {
            PubMedId = pubMedId;
            Authors = authors;
            PublicationDate = publicationDate;
            ReferencedPubMedIds = referencedPubMedIds;
        }

        /// <summary>
        /// Gets or sets the PubMed ID of the publication.
        /// </summary>
        public string? PubMedId { get; set; }

        /// <summary>
        /// Gets or sets the list of authors associated with the publication.
        /// </summary>
        public List<ExpertDto>? Authors { get; set; }

        /// <summary>
        /// Gets or sets the publication date of the publication.
        /// </summary>
        public string? PublicationDate { get; set; }

        /// <summary>
        /// Gets or sets the list of PubMed IDs referenced by the publication.
        /// </summary>
        public List<string>? ReferencedPubMedIds { get; set; }

        /// <summary>
        /// Gets or sets the count of citations for the publication.
        /// </summary>
        public int CitationsCount { get; set; }
    }
}
