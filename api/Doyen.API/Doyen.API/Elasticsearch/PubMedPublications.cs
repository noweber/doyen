namespace QuickType
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using System;
    using System.Globalization;

    public partial class PubMedPublications
    {
        [JsonProperty("took")]
        public long Took { get; set; }

        [JsonProperty("timed_out")]
        public bool TimedOut { get; set; }

        [JsonProperty("_shards")]
        public Shards Shards { get; set; }

        [JsonProperty("hits")]
        public Hits Hits { get; set; }
    }

    public partial class Hits
    {
        [JsonProperty("total")]
        public Total Total { get; set; }

        [JsonProperty("max_score")]
        public double MaxScore { get; set; }

        [JsonProperty("hits")]
        public Hit[] HitsHits { get; set; }
    }

    public partial class Hit
    {
        [JsonProperty("_index")]
        public Index Index { get; set; }

        [JsonProperty("_id")]
        public string Id { get; set; }

        [JsonProperty("_score")]
        public double Score { get; set; }

        [JsonProperty("_ignored", NullValueHandling = NullValueHandling.Ignore)]
        public string[] Ignored { get; set; }

        [JsonProperty("_source")]
        public Source Source { get; set; }
    }

    public partial class Source
    {
        [JsonProperty("pmid")]
        [JsonConverter(typeof(ParseStringConverter))]
        public long Pmid { get; set; }

        [JsonProperty("pii")]
        public string Pii { get; set; }

        [JsonProperty("doi")]
        public string Doi { get; set; }

        [JsonProperty("pmcid")]
        public string Pmcid { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("authors")]
        public Author[] Authors { get; set; }

        [JsonProperty("page")]
        public string Page { get; set; }

        [JsonProperty("journal_title")]
        public string JournalTitle { get; set; }

        [JsonProperty("journal_abbrev")]
        public string JournalAbbrev { get; set; }

        [JsonProperty("issn_list")]
        public string[] IssnList { get; set; }

        [JsonProperty("journal_nlm_id")]
        [JsonConverter(typeof(ParseStringConverter))]
        public long JournalNlmId { get; set; }

        [JsonProperty("mesh_annotations")]
        public MeshAnnotation[] MeshAnnotations { get; set; }

        [JsonProperty("publication_date")]
        public DateTimeOffset PublicationDate { get; set; }

        [JsonProperty("abstract")]
        public string Abstract { get; set; }
    }

    public partial class Author
    {
        [JsonProperty("last_name")]
        public string LastName { get; set; }

        [JsonProperty("first_name")]
        public string FirstName { get; set; }

        [JsonProperty("initials")]
        public string Initials { get; set; }

        [JsonProperty("suffix")]
        public object Suffix { get; set; }

        [JsonProperty("identifier")]
        public string Identifier { get; set; }

        [JsonProperty("affiliations")]
        public Affiliation[] Affiliations { get; set; }
    }

    public partial class Affiliation
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("identifiers")]
        public object[] Identifiers { get; set; }
    }

    public partial class MeshAnnotation
    {
        [JsonProperty("type")]
        public TypeEnum Type { get; set; }

        [JsonProperty("mesh")]
        public string Mesh { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("major_topic")]
        public bool MajorTopic { get; set; }

        [JsonProperty("qualifier")]
        public Qualifier Qualifier { get; set; }

        [JsonProperty("qualifiers")]
        public Qualifier[] Qualifiers { get; set; }
    }

    public partial class Qualifier
    {
        [JsonProperty("text")]
        public Text Text { get; set; }

        [JsonProperty("mesh")]
        public Mesh Mesh { get; set; }
    }

    public partial class Total
    {
        [JsonProperty("value")]
        public long Value { get; set; }

        [JsonProperty("relation")]
        public string Relation { get; set; }
    }

    public partial class Shards
    {
        [JsonProperty("total")]
        public long Total { get; set; }

        [JsonProperty("successful")]
        public long Successful { get; set; }

        [JsonProperty("skipped")]
        public long Skipped { get; set; }

        [JsonProperty("failed")]
        public long Failed { get; set; }
    }

    public enum Index { PubmedPaperIndex };

    public enum Mesh { Q000000981, Q000175 };

    public enum Text { Diagnosis, DiagnosticImaging };

    public enum TypeEnum { Main };

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                IndexConverter.Singleton,
                MeshConverter.Singleton,
                TextConverter.Singleton,
                TypeEnumConverter.Singleton,
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }

    internal class IndexConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(Index) || t == typeof(Index?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            if (value == "pubmed-paper-index")
            {
                return Index.PubmedPaperIndex;
            }
            throw new Exception("Cannot unmarshal type Index");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (Index)untypedValue;
            if (value == Index.PubmedPaperIndex)
            {
                serializer.Serialize(writer, "pubmed-paper-index");
                return;
            }
            throw new Exception("Cannot marshal type Index");
        }

        public static readonly IndexConverter Singleton = new IndexConverter();
    }

    internal class ParseStringConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(long) || t == typeof(long?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            long l;
            if (Int64.TryParse(value, out l))
            {
                return l;
            }
            throw new Exception("Cannot unmarshal type long");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (long)untypedValue;
            serializer.Serialize(writer, value.ToString());
            return;
        }

        public static readonly ParseStringConverter Singleton = new ParseStringConverter();
    }

    internal class MeshConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(Mesh) || t == typeof(Mesh?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            switch (value)
            {
                case "Q000000981":
                    return Mesh.Q000000981;
                case "Q000175":
                    return Mesh.Q000175;
            }
            throw new Exception("Cannot unmarshal type Mesh");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (Mesh)untypedValue;
            switch (value)
            {
                case Mesh.Q000000981:
                    serializer.Serialize(writer, "Q000000981");
                    return;
                case Mesh.Q000175:
                    serializer.Serialize(writer, "Q000175");
                    return;
            }
            throw new Exception("Cannot marshal type Mesh");
        }

        public static readonly MeshConverter Singleton = new MeshConverter();
    }

    internal class TextConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(Text) || t == typeof(Text?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            switch (value)
            {
                case "diagnosis":
                    return Text.Diagnosis;
                case "diagnostic imaging":
                    return Text.DiagnosticImaging;
            }
            throw new Exception("Cannot unmarshal type Text");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (Text)untypedValue;
            switch (value)
            {
                case Text.Diagnosis:
                    serializer.Serialize(writer, "diagnosis");
                    return;
                case Text.DiagnosticImaging:
                    serializer.Serialize(writer, "diagnostic imaging");
                    return;
            }
            throw new Exception("Cannot marshal type Text");
        }

        public static readonly TextConverter Singleton = new TextConverter();
    }

    internal class TypeEnumConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(TypeEnum) || t == typeof(TypeEnum?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            if (value == "main")
            {
                return TypeEnum.Main;
            }
            throw new Exception("Cannot unmarshal type TypeEnum");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (TypeEnum)untypedValue;
            if (value == TypeEnum.Main)
            {
                serializer.Serialize(writer, "main");
                return;
            }
            throw new Exception("Cannot marshal type TypeEnum");
        }

        public static readonly TypeEnumConverter Singleton = new TypeEnumConverter();
    }
}
