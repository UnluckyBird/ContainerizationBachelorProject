using KubernetesAPI.Data;
using KubernetesAPI.Models.DBModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KubernetesAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ImageController : Controller
    {
        private readonly ILogger<ImageController> _logger;
        private readonly ApplicationDbContext _db;

        public ImageController(ILogger<ImageController> logger, ApplicationDbContext db)
        {
            _logger = logger;
            _db = db;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Image>>> Get()
        {
            List<Image> images = await _db.Image.ToListAsync();

            return images;
        }
    }
}
