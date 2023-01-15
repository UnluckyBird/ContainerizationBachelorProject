namespace KubernetesAPI.Models
{
    public class Connector
    {
        public string? Name { get; set; }

        public string? Type { get; set; }

        public string? Image { get; set; }

        public int? Replicas { get; set; }

        public int? AvaiableReplicas { get; set; }
    }
}