namespace KubernetesAPI.Models.DTO.Post
{
    public class ConnectorPostDTO
    {
        public string? DeploymentName { get; set; }

        public string? Type { get; set; }

        public string? Image { get; set; }

        public int? Replicas { get; set; }

        public bool? CreateService { get; set; }

        public int? ExternalPortNumber { get; set; }
    }
}
