namespace KubernetesAPI.Models.DTO.Get
{
    public class ConnectorTypeDTO
    {
        public string Type { get; set; }

        public string Repository { get; set; }

        public int? MaxReplicas { get; set; }

        public IList<int> ExposedPorts { get; set; }

        public IList<string> Images { get; set; }
    }
}
