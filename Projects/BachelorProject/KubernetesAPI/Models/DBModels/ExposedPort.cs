using System.ComponentModel.DataAnnotations;

namespace KubernetesAPI.Models.DBModels
{
    public class ExposedPort
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int Port { get; set; }
    }
}
