using KubernetesAPI.Data;
using KubernetesAPI.Models.DBModels;
using KubernetesAPI.Models.DTO.Get;
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
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ImageDTO>>> Get()
        {
            List<ImageDTO> images = await _db.Image.Include(i => i.ConnectorType).Select(i => new ImageDTO()
            {
                ConnectorType = i.ConnectorType.Type,
                Tag= i.Tag,
                Digest = i.Digest,
                LastPushed = i.LastPushed
            }).ToListAsync();

            return images;
        }
    }
}
