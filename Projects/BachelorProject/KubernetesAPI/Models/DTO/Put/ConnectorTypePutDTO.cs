namespace KubernetesAPI.Models.DTO.Put
{
    public class ConnectorTypePutDTO
    {
        public string Type { get; set; }

        public string Repository { get; set; }

        public int? MaxReplicas { get; set; }

        public List<int> ExposedPorts { get; set; }
    }
}
