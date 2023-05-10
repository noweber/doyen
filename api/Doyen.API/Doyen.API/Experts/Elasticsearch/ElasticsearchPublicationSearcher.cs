using Doyen.API.Dtos.Requests;
using Doyen.API.Dtos.Responses;
using Doyen.API.Experts.Elasticsearch.Http;
using Doyen.API.Experts.Elasticsearch.Settings;
using Newtonsoft.Json.Linq;
using System.Globalization;

namespace Doyen.API.Experts.Elasticsearch
{
    public sealed class ElasticsearchPublicationSearcher : IExpertSearcher
    {
        private readonly IElasticsearchSettings settings;

        private readonly IElasticsearchHttpClient elasticHttpClient;

        private const string DATE_TIME_FORMAT = "yyyy-MM-dd";

        public ElasticsearchPublicationSearcher(IElasticsearchSettings elasticsearchSettings, IElasticsearchHttpClient httpClient)
        {
            settings = elasticsearchSettings ?? throw new ArgumentNullException(nameof(elasticsearchSettings));
            elasticHttpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public List<ExpertMetricsDtos> GetExpertMetricsByKeywords(SearchDto searchQuery, TimeRangeDto timeRange)
        {
            if (searchQuery == null)
            {
                throw new ArgumentNullException(nameof(searchQuery));
            }
            if (timeRange == null)
            {
                throw new ArgumentNullException(nameof(timeRange));
            }

            string requestJson =
            "{\"size\": " + settings.SearchRecordsLimit + ",\"query\":{\"bool\":{\"must\":[{\"simple_query_string\":{\"default_operator\":\"and\",\"fields\":[\"title\",\"abstract\",\"mesh_annotations.text\"],\"query\":\"" + searchQuery.Keywords + "\"}},{\"range\":{\"publication_date\":{\"gte\":\"" + timeRange.GreaterThan.ToString(DATE_TIME_FORMAT) + "\",\"lte\":\"" + timeRange.LessThan.ToString(DATE_TIME_FORMAT) + "\"}}}]}}}";
            string responseJson = elasticHttpClient.SendSearchPostRequest(requestJson).Result;
            dynamic responseObject = JObject.Parse(responseJson);
            dynamic hits = responseObject.hits.hits;

            Dictionary<string, ExpertMetricsDtos> experts = new();
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
                            var expert = new ExpertMetricsDtos(firstName, lastName, identifier);
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
            List<PublicationDto> publications = GetPublicationsFromSubhits(hits);
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

            List<ExpertMetricsDtos> results = new();
            foreach (var expert in experts)
            {
                results.Add(expert.Value);
            }

            return results;
        }

        public ExpertDetailsDto GetExpertDetailsByIdentifier(string identifier)
        {
            string requestJson = "{\"query\":{\"bool\":{\"should\":[{\"term\":{\"authors.identifier.keyword\":{\"value\":\"" + identifier + "\"}}},{\"terms\":{\"authors.identifier.keyword\":[\"" + identifier + "\"]}}]}}}";
            string responseJson = elasticHttpClient.SendSearchPostRequest(requestJson).Result;
            return GetExpertDetailsByIdentifierFromElasticSearchDocumentsJson(responseJson, identifier);
        }

        private ExpertDetailsDto GetExpertDetailsByIdentifierFromElasticSearchDocumentsJson(string documentsJson, string identifier)
        {
            dynamic responseObject = JObject.Parse(documentsJson);
            dynamic hits = responseObject.hits.hits;
            var publications = GetPublicationsFromSubhits(hits);
            var expert = GetExpertByIdFromSubhits(hits, identifier);
            return new ExpertDetailsDto(expert.Name, identifier, publications);
        }

        public List<CollaboratorDto> GetCollaboratorsByExpertIdentifer(string identifer)
        {
            // Retrieve expert details by the provided identifier
            var expertDetails = GetExpertDetailsByIdentifier(identifer);

            // Initialize collaboration counts and collaborators seen
            Dictionary<ExpertDto, int> collaborationCounts = new();
            HashSet<string> collaboratorsSeen = new();

            if (expertDetails != null && expertDetails.Publications != null)
            {
                // Count the collaborations for each publication and author
                foreach (var publication in expertDetails.Publications)
                {
                    if (publication.Authors != null)
                    {
                        foreach (var author in publication.Authors)
                        {
                            if (author != null)
                            {
                                if (collaborationCounts.ContainsKey(author))
                                {
                                    collaborationCounts[author] += 1;
                                }
                                else
                                {
                                    collaborationCounts.TryAdd(author, 1);
                                }
                            }
                        }
                    }
                }
            }

            // Create a list of collaborators sorted by the number of collaborations
            List<CollaboratorDto> collaborators = new List<CollaboratorDto>();
            foreach (var entry in collaborationCounts)
            {
                collaborators.Add(new CollaboratorDto(entry.Key, entry.Value));
            }

            return collaborators;
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

        private List<PublicationDto> GetPublicationsFromSubhits(dynamic subhits)
        {
            List<PublicationDto> results = new();
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

        private PublicationDto GetPublicationFromSubhitsSource(dynamic source)
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
            return new PublicationDto(pubMedId, experts, publicationDate, references);
        }

        private List<ExpertDto> GetExpertsFromHitSourceAuthors(dynamic authors)
        {
            List<ExpertDto> results = new();
            foreach (var author in authors)
            {
                results.Add(GetExpertFromAuthor(author));
            }
            return results;
        }

        private ExpertDto GetExpertFromAuthor(dynamic author)
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
            return new ExpertDto(firstName, lastName, identifier);
        }

        private ExpertDto GetExpertByIdFromSubhits(dynamic subhits, string identifier)
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
                        return new ExpertDto(firstName, lastName, id);
                    }
                }
            }
            return null;
        }
    }

}
