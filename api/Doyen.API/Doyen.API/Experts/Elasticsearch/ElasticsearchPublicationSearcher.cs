using Doyen.API.Dtos;
using Doyen.API.Experts.Elasticsearch.Http;
using Doyen.API.Experts.Elasticsearch.Settings;
using Doyen.API.Logging;
using Doyen.API.Models;
using Newtonsoft.Json.Linq;
using System.Globalization;

namespace Doyen.API.Experts.Elasticsearch
{
    public sealed class ElasticsearchPublicationSearcher : IExpertSearcher
    {
        private readonly IElasticsearchSettings settings;

        private readonly IElasticsearchHttpClient elasticHttpClient;

        private const string DATE_TIME_FORMAT = "yyyy-MM-dd";

        public ElasticsearchPublicationSearcher(IElasticsearchSettings elasticsearchSettings, ITraceLogger traceLogger, IElasticsearchHttpClient httpClient)
        {
            settings = elasticsearchSettings ?? throw new ArgumentNullException(nameof(elasticsearchSettings));
            elasticHttpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public List<ExpertMetrics> GetExpertMetricsByKeywords(SearchModel searchQuery, TimeRangeModel timeRange)
        {
            string requestJson =
            "{\"size\": " + settings.SearchRecordsLimit + ",\"query\":{\"bool\":{\"must\":[{\"simple_query_string\":{\"default_operator\":\"and\",\"fields\":[\"title\",\"abstract\",\"mesh_annotations.text\"],\"query\":\"" + searchQuery.Keywords + "\"}},{\"range\":{\"publication_date\":{\"gte\":\"" + timeRange.GreaterThan.ToString(DATE_TIME_FORMAT) + "\",\"lte\":\"" + timeRange.LessThan.ToString(DATE_TIME_FORMAT) + "\"}}}]}}}";
            string responseJson = elasticHttpClient.SendSearchPostRequest(requestJson).Result;
            dynamic responseObject = JObject.Parse(responseJson);
            dynamic hits = responseObject.hits.hits;

            Dictionary<string, ExpertMetrics> experts = new();
            foreach (var subhit in hits)
            {

                foreach (var author in subhit._source.authors)
                {
                    string firstName = author.first_name;
                    string lastName = author.last_name;
                    string identifier = author.identifier;
                    var key = GetKeyByFirstNameLastNameAndIdentifier(firstName, lastName, identifier);
                    if (!string.IsNullOrEmpty(key))
                    {
                        string score = subhit._score;
                        score = score[1..^1];
                        float relevancy = float.Parse(score, CultureInfo.InvariantCulture);

                        if (experts.ContainsKey(key))
                        {

                            experts[key].AddToRelevancySum(relevancy);
                            experts[key].IncrementPublicationsCount();
                        }
                        else
                        {
                            var expert = new ExpertMetrics(firstName, lastName, identifier);
                            if (experts.TryAdd(key, expert))
                            {
                                if (subhit._score != null)
                                {
                                    experts[key].AddToRelevancySum(relevancy);
                                }

                                experts[key].IncrementPublicationsCount();
                            }
                        }
                    }
                }

            }

            // Calculate the citations count for each author's publications:
            List<Publication> publications = GetPublicationsFromSubhits(hits);
            foreach (var expert in experts)
            {
                if (publications != null)
                {
                    foreach (var publication in publications)
                    {
                        if (publication != null && publication.Authors != null)
                        {
                            foreach (var author in publication.Authors)
                            {
                                //string authorKey = GetKeyByFirstNameLastNameAndIdentifier(author.Name, author.Name, author.Identifier);
                                //string expertKey = GetKeyByFirstNameLastNameAndIdentifier(author.Name, author.Name, author.Identifier);
                                if (string.Equals(author.Name, expert.Value.Name))
                                {
                                    if (string.IsNullOrEmpty(expert.Value.Identifier) ||
                                        !string.IsNullOrEmpty(expert.Value.Identifier) && string.Equals(author.Identifier, expert.Value.Identifier))
                                    {
                                        expert.Value.AddToCitationsCount(publication.CitationsCount);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            List<ExpertMetrics> results = new();
            foreach (var expert in experts)
            {
                results.Add(expert.Value);
            }

            return results;
        }

        public ExpertDetails GetExpertDetailsByIdentifier(string identifier)
        {
            string requestJson = "{\"query\":{\"bool\":{\"should\":[{\"term\":{\"authors.identifier.keyword\":{\"value\":\"" + identifier + "\"}}},{\"terms\":{\"authors.identifier.keyword\":[\"" + identifier + "\"]}}]}}}";
            string responseJson = elasticHttpClient.SendSearchPostRequest(requestJson).Result;
            return GetExpertDetailsByIdentifierFromElasticSearchDocumentsJson(responseJson, identifier);
        }

        private ExpertDetails GetExpertDetailsByIdentifierFromElasticSearchDocumentsJson(string documentsJson, string identifier)
        {
            dynamic responseObject = JObject.Parse(documentsJson);
            dynamic hits = responseObject.hits.hits;
            var publications = GetPublicationsFromSubhits(hits);
            var expert = GetExpertByIdFromSubhits(hits, identifier);
            return new ExpertDetails(expert.Name, identifier, publications);
        }

        public List<Collaborator> GetCollaboratorsByExpertIdentifer(string identifer)
        {
            throw new NotImplementedException();
        }

        private string GetKeyByFirstNameLastNameAndIdentifier(string firstName, string lastName, string identifier)
        {
            string result = string.Empty;
            if (!string.IsNullOrEmpty(firstName) && !string.IsNullOrEmpty(lastName))
            {
                result = firstName + ":" + lastName;
                if (!string.IsNullOrEmpty(identifier))
                {
                    result += ":" + identifier;
                }
            }
            return result;
        }

        private string? GetAuthorKeyFromSourceAuthor(dynamic author)
        {
            string firstName = author.first_name;
            string lastName = author.last_name;
            string identifier = author.identifier;
            string? key = null;
            if (!string.IsNullOrEmpty(firstName) && !string.IsNullOrEmpty(lastName))
            {
                key = firstName + ":" + lastName;
                if (!string.IsNullOrEmpty(identifier))
                {
                    key += ":" + identifier;
                }
            }
            return key;
        }

        private List<Publication> GetPublicationsFromSubhits(dynamic subhits)
        {
            List<Publication> results = new();
            foreach (var subhit in subhits)
            {
                if (subhit != null && subhit._source != null)
                {
                    results.Add(GetPublicationFromSubhitsSource(subhit._source));

                }
            }

            Dictionary<string, int> citationsCountByPubMedId = new Dictionary<string, int>();
            foreach (var publication in results)
            {
                if (publication.PubMedId != null)
                {
                    if (citationsCountByPubMedId.ContainsKey(publication.PubMedId))
                    {
                        citationsCountByPubMedId[publication.PubMedId] += 1;
                    }
                    else
                    {
                        citationsCountByPubMedId.TryAdd(publication.PubMedId, 1);
                    }
                }
            }
            foreach (var publication in results)
            {
                if (publication.PubMedId != null)
                {
                    if (citationsCountByPubMedId.ContainsKey(publication.PubMedId))
                    {
                        publication.CitationsCount = citationsCountByPubMedId[publication.PubMedId];
                    }
                }
            }
            return results;
        }

        private Publication GetPublicationFromSubhitsSource(dynamic source)
        {
            string pubMedId = string.Empty;
            if (source.pmid != null)
            {
                pubMedId = source.pmid;
            }
            var experts = GetExpertsFromHitSourceAuthors(source.authors);
            string publicationDate = string.Empty;
            if (source.publication_date != null)
            {
                publicationDate = source.publication_date;
            }
            List<string> references = new List<string>();
            if (source.references != null)
            {
                foreach (var reference in source.references)
                {
                    if (reference != null)
                    {
                        references.Add(reference.ToString());
                    }
                }
            }
            return new Publication(pubMedId, experts, publicationDate, references);
        }

        private List<Expert> GetExpertsFromHitSourceAuthors(dynamic authors)
        {
            List<Expert> results = new();
            foreach (var author in authors)
            {
                results.Add(GetExpertFromAuthor(author));
            }
            return results;
        }

        private Expert GetExpertFromAuthor(dynamic author)
        {
            string? firstName = null;
            if (author.first_name != null)
            {
                firstName = author.first_name;
            }
            string? lastName = null;
            if (author.last_name != null)
            {
                lastName = author.last_name;
            }
            string? identifier = null;
            if (author.identifier != null)
            {
                identifier = author.identifier;
            }
            return new Expert(firstName, lastName, identifier);
        }

        private Expert GetExpertByIdFromSubhits(dynamic subhits, string identifier)
        {
            foreach (var subhit in subhits)
            {
                var authors = subhit._source.authors;
                foreach (var author in authors)
                {
                    if (author.identifier != null && author.identifier == identifier)
                    {
                        string? firstName = null;
                        if (author.first_name != null)
                        {
                            firstName = author.first_name;
                        }
                        string? lastName = null;
                        if (author.last_name != null)
                        {
                            lastName = author.last_name;
                        }
                        string? id = null;
                        if (author.identifier != null)
                        {
                            id = author.identifier;
                        }
                        return new Expert(firstName, lastName, id);
                    }
                }
            }
            return null;
        }
    }

}
