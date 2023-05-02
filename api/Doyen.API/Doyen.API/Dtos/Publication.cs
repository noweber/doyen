namespace Doyen.API.Dtos
{
    public class Publication
    {
        public Publication(string? pubMedId, List<Expert>? authors)// string? publicationDate)
        {
            PubMedId = pubMedId;
            Authors = authors;
            //PublicationDate = publicationDate;
        }

        public string? PubMedId { get; set; }

        public List<Expert>? Authors { get; set; }

        //public string? PublicationDate { get; set; }
    }
}
