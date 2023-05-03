namespace Doyen.API.Dtos
{
    public class Publication
    {
        public Publication(string? pubMedId = null, List<Expert>? authors = null, string? publicationDate = null, List<string>? referencedPubMedIds = null)
        {
            PubMedId = pubMedId;
            Authors = authors;
            PublicationDate = publicationDate;
            ReferencedPubMedIds = referencedPubMedIds;
        }

        public string? PubMedId { get; set; }

        public List<Expert>? Authors { get; set; }

        public string? PublicationDate { get; set; }

        public List<string>? ReferencedPubMedIds { get; set; }

        public int CitationsCount { get; set; }
    }
}
