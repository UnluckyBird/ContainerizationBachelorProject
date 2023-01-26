namespace KubernetesAPI.Models.DTO.Get
{
    public class ImageDTO
    {
        public string? ConnectorType { get; set; }

        public string Tag { get; set; }

        public string Digest { get; set; }

        public DateTime? LastPushed { get; set; }
    }
}
