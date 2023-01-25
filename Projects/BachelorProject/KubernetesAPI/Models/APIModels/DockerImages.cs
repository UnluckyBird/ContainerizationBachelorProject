using System.Text.Json.Serialization;

namespace KubernetesAPI.Models.APIModels
{
    public class DockerImages
    {
        public int Count { get; set; }

        public string Next { get; set; }

        public string Previous { get; set; }

        public List<Result> Results { get; set; }
    }
    public class Result
    {
        public string NameSpace { get; set; }

        public string Repository { get; set; }

        public string Digest { get; set; }

        public List<Tag> Tags { get; set; }

        [JsonPropertyName("last_pushed")]
        public DateTime LastPushed { get; set; }

        [JsonPropertyName("last_pulled")]
        public DateTime? LastPulled { get; set; }

        public string Status { get; set; }
    }

    public class Tag
    {
        [JsonPropertyName("tag")]
        public string TagName { get; set; }

        [JsonPropertyName("is_current")]
        public bool IsCurrent { get; set; }
    }
}
