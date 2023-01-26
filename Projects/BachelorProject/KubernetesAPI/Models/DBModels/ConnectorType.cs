using System.ComponentModel.DataAnnotations;

namespace KubernetesAPI.Models.DBModels
{
    public class ConnectorType
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Type { get; set; }

        [Required]
        public string Repository { get; set; }

        public int? MaxReplicas { get; set; }

        public IList<Image> Images { get; set; }
    }
}
