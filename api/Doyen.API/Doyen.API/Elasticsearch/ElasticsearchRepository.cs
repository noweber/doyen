using Nest;

namespace Doyen.API.Elasticsearch
{
    public class ElasticsearchRepository
    {
        const string pubmedIndex = "pubmed-paper-index";

        ElasticClient client = null;
        public ElasticsearchRepository()
        {
            var uri = new Uri("http://localhost:9200");
            var settings = new ConnectionSettings(uri);
            client = new ElasticClient(settings);
            settings.DefaultIndex(pubmedIndex);
        }

        public List<string> GetPublicationsByKeywords(string keywords)
        {
            /*
            var response = client.Get<List<Publication>>(10, idx => idx.Index(pubmedIndex));
            if (response.IsValid)
            {
                return response.Source;
            }
            return new List<Publication>();*/
            /*
        var response = client.Search<string>(s => s
                .From(0)
                .Take(10)
                .Query(qry => qry
                    .Bool(b => b
                        .Must(m => m
                            .QueryString(qs => qs
                                .DefaultField("_all")
                                .Query(keywords)))))).Documents.ToList();*/
            //return response.Documents.ToList();
            return null;
        }

        /*
        public List<City> GetResult()
        {
            if (client.IndexExists(pubmedIndex).Exists)
            {
                var response = client.Search<City>();
                return response.Documents.ToList();
            }
            return null;
        }

        public List<City> GetResult(string condition)
        {
            if (client.IndexExists(pubmedIndex).Exists)
            {
                var query = condition;

                return client.SearchAsync<City>(s => s
                    .From(0)
                    .Take(10)
                    .Query(qry => qry
                        .Bool(b => b
                            .Must(m => m
                                .QueryString(qs => qs
                                    .DefaultField("_all")
                                    .Query(query)))))).Result.Documents.ToList();
            }
            return null;
        }*/
    }
}
