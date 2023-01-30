using KubernetesAPI.Data;
using KubernetesAPI.Models.DTO.Get;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Net.Http;
using Microsoft.Extensions.Options;
using KubernetesAPI.Settings;

namespace KubernetesAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PodController : Controller
    {
        private readonly ILogger<ImageController> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public PodController(ILogger<ImageController> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet("{podName}/Logs")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<string>> GetLogs(string podName)
        {
            _logger.LogInformation("Getting pod logs");

            HttpClient httpClient = _httpClientFactory.CreateClient("kubePodsClient");
            if (System.IO.File.Exists("/var/run/secrets/kubernetes.io/serviceaccount/token"))
            {
                string token = System.IO.File.ReadAllText("/var/run/secrets/kubernetes.io/serviceaccount/token");
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            try
            {
                var response = await httpClient.GetAsync($"pods/{podName}/log?timestamps=true");

                response.EnsureSuccessStatusCode();

                string log = await response.Content.ReadAsStringAsync();
                return log;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return BadRequest(ex.Message);
            }
            
        }
    }
}
