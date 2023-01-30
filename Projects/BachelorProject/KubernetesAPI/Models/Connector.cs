namespace KubernetesAPI.Models
{
    public class Connector
    {
        public string? DeploymentName { get; set; }

        public DateTime? DeploymentTime { get; set; }

        public string? Type { get; set; }

        public string? Image { get; set; }

        public int? Replicas { get; set; }

        public int? AvailableReplicas { get; set; }

        public List<EnvironmentVariable> Env { get; set; } = new List<EnvironmentVariable>();
    }

    public class EnvironmentVariable
    {
        public string? Name { get; set;}

        public string? Value { get; set;}
    }
}