using System.ComponentModel.DataAnnotations;

namespace KubernetesAPI.Models.DBModels
{
    public class Image
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Tag { get; set; } = string.Empty;

        [Required]
        public string Digest { get; set; } = string.Empty;

        public DateTime? LastPushed { get; set; }

        public ConnectorType? ConnectorType { get; set; }
    }
}
