using System.ComponentModel.DataAnnotations;

namespace KubernetesAPI.Models.DBModels
{
    public class ConnectorType
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Type { get; set; } = string.Empty;

        [Required]
        public string Repository { get; set; } = string.Empty;

        public int? MaxReplicas { get; set; }

        public IList<ExposedPort> ExposedPorts { get; set; } = new List<ExposedPort>();

        public IList<Image> Images { get; set; } = new List<Image>();
    }
}
