using System.ComponentModel.DataAnnotations;

namespace KubernetesAPI.Models.DBModels
{
    public class ConnectorType
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public int MaxReplicas { get; set; }

        public IList<Image> Images { get; set; }
    }
}
