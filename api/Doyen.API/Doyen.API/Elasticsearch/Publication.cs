namespace Doyen.API.Elasticsearch
{
    public class Publication
    {
        public string publication_date { get; set; }

        public List<Author> authors { get; set; }

        public string title { get; set; }
    }
}
