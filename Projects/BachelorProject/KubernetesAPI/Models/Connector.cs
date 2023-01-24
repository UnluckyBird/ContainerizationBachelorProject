namespace KubernetesAPI.Models
{
    public class Connector
    {
        public string? DeploymentName { get; set; }

        public string? Label { get; set; }

        public string? Type { get; set; }

        public string? Image { get; set; }

        public int? Replicas { get; set; }

        public int? AvailableReplicas { get; set; }
    }
}