namespace KubernetesAPI.Models.DTO.Patch
{
    public class ConnectorPatchDTO
    {
        public string? DeploymentName { get; set; }

        public string? Type { get; set; }

        public string? Image { get; set; }

        public int? Replicas { get; set; }
    }
}
