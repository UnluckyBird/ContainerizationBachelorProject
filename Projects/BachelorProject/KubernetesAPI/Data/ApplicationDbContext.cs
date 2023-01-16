using KubernetesAPI.Models.DBModels;
using Microsoft.EntityFrameworkCore;

namespace KubernetesAPI.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }

        public DbSet<ConnectorType> ConnectorType { get; set; }
        public DbSet<Image> Image { get; set; }
    }
}
