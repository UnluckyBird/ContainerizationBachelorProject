using System.ComponentModel.DataAnnotations;

namespace KubernetesAPI.Models.DBModels
{
    public class Image
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Repository { get; set; }

        public DateTime? ReleaseDate { get; set; }

        public ConnectorType? ConnectorType { get; set; }
    }
}
