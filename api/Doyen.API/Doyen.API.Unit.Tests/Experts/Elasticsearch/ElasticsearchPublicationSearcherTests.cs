using Doyen.API.Dtos.Requests;
using Doyen.API.Experts.Elasticsearch.Http;
using Doyen.API.Experts.Elasticsearch.Settings;
using Moq;
using Newtonsoft.Json.Linq;

namespace Doyen.API.Experts.Elasticsearch.Tests
{
    [TestClass]
    public class ElasticsearchPublicationSearcherTests
    {
        private readonly IElasticsearchSettings elasticsearchSettingsMock = Mock.Of<IElasticsearchSettings>();
        private readonly IElasticsearchHttpClient elasticHttpClientMock = Mock.Of<IElasticsearchHttpClient>();

        [TestMethod]
        public void GetExpertMetricsByKeywords_ReturnsListOfExpertMetrics()
        {
            // Arrange
            var searcher = new ElasticsearchPublicationSearcher(elasticsearchSettingsMock, elasticHttpClientMock);
            var searchQuery = new SearchDto { Keywords = "test" };
            var timeRange = new TimeRangeDto { GreaterThan = DateTime.Today.AddDays(-7), LessThan = DateTime.Today };

            // Mock the response from Elasticsearch
            var expectedResponse = GetSampleElasticsearchResponse();
            MockElasticsearchHttpClientResponse(expectedResponse);

            // Act
            var result = searcher.GetExpertMetricsByKeywords(searchQuery, timeRange);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
        }

        [TestMethod]
        public void GetExpertMetricsByKeywords_ThrowsArgumentNullException_WhenSearchQueryIsNull()
        {
            // Arrange
            var searcher = new ElasticsearchPublicationSearcher(elasticsearchSettingsMock, elasticHttpClientMock);
            SearchDto searchQuery = null;
            var timeRange = new TimeRangeDto { GreaterThan = DateTime.Today.AddDays(-7), LessThan = DateTime.Today };

            // Act and Assert
            Assert.ThrowsException<ArgumentNullException>(() => searcher.GetExpertMetricsByKeywords(searchQuery, timeRange));
        }

        [TestMethod]
        public void GetExpertMetricsByKeywords_ThrowsArgumentNullException_WhenTimeRangeIsNull()
        {
            // Arrange
            var searcher = new ElasticsearchPublicationSearcher(elasticsearchSettingsMock, elasticHttpClientMock);
            var searchQuery = new SearchDto { Keywords = "test" };
            TimeRangeDto timeRange = null;

            // Act and Assert
            Assert.ThrowsException<ArgumentNullException>(() => searcher.GetExpertMetricsByKeywords(searchQuery, timeRange));
        }

        private void MockElasticsearchHttpClientResponse(string responseJson)
        {
            Mock.Get(elasticHttpClientMock)
                .Setup(client => client.SendSearchPostRequest(It.IsAny<string>()))
                .ReturnsAsync(responseJson);
        }

        public static string GetSampleElasticsearchResponse()
        {
            Random random = new Random();

            JObject response = new JObject(
                new JProperty("took", random.Next(50, 100)),
                new JProperty("timed_out", random.Next(2) == 0 ? true : false),
                new JProperty("_shards",
                    new JObject(
                        new JProperty("total", 1),
                        new JProperty("successful", 1),
                        new JProperty("skipped", 0),
                        new JProperty("failed", 0)
                    )
                ),
                new JProperty("hits",
                    new JObject(
                        new JProperty("total",
                            new JObject(
                                new JProperty("value", random.Next(1000, 10000)),
                                new JProperty("relation", random.Next(2) == 0 ? "eq" : "gte")
                            )
                        ),
                        new JProperty("max_score", random.NextDouble() * 20),
                        new JProperty("hits",
                            new JArray(
                                GetRandomHit(random),
                                GetRandomHit(random)
                    )
                )
            )
        )
    );

            return response.ToString();
        }

        private static JObject GetRandomHit(Random random)
        {
            return new JObject(
                        new JProperty("_index", "pubmed-paper-index"),
                        new JProperty("_id", "yFIT9IcBEK6nshxONzdF"),
                        new JProperty("_score", random.NextDouble() * 20),
                        new JProperty("_source",
                            new JObject(
                                new JProperty("pmid", random.Next(10000000, 99999999).ToString()),
                                new JProperty("pii", "S" + random.Next(1000000000, 1999999999) + "-1"),
                                new JProperty("doi", "10." + random.Next(1000000, 9999999) + "/j.vaccine." + random.Next(10, 99) + "." + random.Next(10, 99)),
                                new JProperty("pmcid", "PMC" + random.Next(1000000, 9999999)),
                                new JProperty("title", "Impact of " + GetRandomWord() + " in " + GetRandomWord() + " " + GetRandomWord() + "."),
                                new JProperty("authors",
                                    new JArray(
                                        new JObject(
                                            new JProperty("last_name", GetRandomName()),
                                            new JProperty("first_name", GetRandomName()),
                                            new JProperty("initials", GetRandomInitials()),
                                            new JProperty("suffix", null),
                                            new JProperty("identifier", null),
                                            new JProperty("affiliations",
                                                new JArray(
                                                    new JObject(
                                                        new JProperty("name", GetRandomAffiliation()),
                                                        new JProperty("identifiers", new JArray())
                                                    )
                                                )
                                            )
                                        )
                                    )
                                ),
                                new JProperty("page", GetRandomPage()),
                                new JProperty("journal_title", GetRandomJournalTitle()),
                                new JProperty("journal_abbrev", GetRandomJournalAbbreviation()),
                                new JProperty("issn_list", new JArray("1873-2518", "0264-410X")),
                                new JProperty("journal_nlm_id", "8406899"),
                                new JProperty("mesh_annotations",
                                    new JArray(
                                        new JObject(
                                            new JProperty("type", "main"),
                                            new JProperty("mesh", "D006801"),
                                            new JProperty("text", GetRandomWord()),
                                            new JProperty("major_topic", false),
                                            new JProperty("qualifier", null),
                                            new JProperty("qualifiers", new JArray())
                                        )
                                    )
                                ), new JProperty("references", new JArray(GetRandomReferences())),
                        new JProperty("publication_date", GetRandomPublicationDate()),
                        new JProperty("abstract", "Publication abstract here.")
                    )
                )
            );
        }

        private static string GetRandomWord()
        {
            string[] words = { "impact", "complications", "vaccination", "diseases", "prevention", "control", "humans", "syndrome" };
            return words[new Random().Next(words.Length)];
        }

        private static string GetRandomName()
        {
            string[] names = { "John", "Jane", "Michael", "Emily", "David", "Sophia", "Daniel", "Olivia" };
            return names[new Random().Next(names.Length)];
        }

        private static string GetRandomInitials()
        {
            string[] initials = { "A", "B", "C", "D", "E", "F", "G", "H" };
            return initials[new Random().Next(initials.Length)];
        }

        private static string GetRandomAffiliation()
        {
            string[] affiliations = { "Institute of Medical Sciences", "Department of Health", "Research Center", "University Hospital" };
            return affiliations[new Random().Next(affiliations.Length)];
        }

        private static string GetRandomPage()
        {
            Random random = new Random();
            int start = random.Next(1000, 2000);
            int end = random.Next(start, 2500);
            return start + "-" + end;
        }

        private static string GetRandomJournalTitle()
        {
            string[] journalTitles = { "Journal of Medicine", "Medical Research Review", "Health Sciences Journal" };
            return journalTitles[new Random().Next(journalTitles.Length)];
        }

        private static string GetRandomJournalAbbreviation()
        {
            string[] journalAbbreviations = { "J Med", "Med Res Rev", "Health Sci J" };
            return journalAbbreviations[new Random().Next(journalAbbreviations.Length)];
        }

        private static string[] GetRandomReferences()
        {
            string[] references = { "32517963", "34907393", "34477808", "32109013" };
            return references;
        }

        private static string GetRandomPublicationDate()
        {
            DateTime startDate = new DateTime(2010, 1, 1);
            int range = (DateTime.Today - startDate).Days;
            DateTime randomDate = startDate.AddDays(new Random().Next(range));
            return randomDate.ToString("yyyy-MM-dd");
        }
    }
}
