using KubernetesAPI.Data;
using KubernetesAPI.Models;
using KubernetesAPI.Models.DBModels;
using KubernetesAPI.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Text;
using System.Net.Http.Headers;
using KubernetesAPI.Models.DTO.Get;

namespace KubernetesAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ConnectorController : ControllerBase
    {
        private readonly ILogger<ConnectorController> _logger;
        private readonly ApplicationDbContext _db;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IOptionsMonitor<KubernetesOptions> _kubernetesOptionsMonitor;

        public ConnectorController(ILogger<ConnectorController> logger, ApplicationDbContext db, IHttpClientFactory httpClientFactory, IOptionsMonitor<KubernetesOptions> kubernetesOptionsMonitor)
        {
            _logger = logger;
            _db = db;
            _httpClientFactory = httpClientFactory;
            _kubernetesOptionsMonitor = kubernetesOptionsMonitor;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Connector))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<Connector>>> Get()
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("kubeClient");
            if (System.IO.File.Exists("/var/run/secrets/kubernetes.io/serviceaccount/token"))
            {
                string token = System.IO.File.ReadAllText("/var/run/secrets/kubernetes.io/serviceaccount/token");
                _logger.LogInformation($"Getting Kubernetes token: {token}");
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            try
            {
                Models.APIModels.Deployments? apiModel = await httpClient.GetFromJsonAsync<Models.APIModels.Deployments>("deployments");

                if(apiModel == null)
                {
                    return NotFound();
                }

                return apiModel.items.Where(d => d.metadata?.nameSpace == "default").Select(d => new Connector
                {
                    DeploymentName = d.metadata?.name ?? "Unknown",
                    Type = d.spec?.template?.spec?.containers.FirstOrDefault()?.name ?? "Unknown",
                    Image = d.spec?.template?.spec?.containers.FirstOrDefault()?.image ?? "Unknown",
                    Replicas = d.status?.replicas ?? 0,
                    AvailableReplicas = d.status?.availableReplicas ?? 0,
                }).ToArray();
            }
            catch(Exception ex)
            {
                _logger.LogError(ex.ToString());
                return BadRequest(ex.Message);
            }         
        }

        [HttpGet("{name}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Connector))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Connector>> Get(string name)
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("kubeClient");
            if (System.IO.File.Exists("/var/run/secrets/kubernetes.io/serviceaccount/token"))
            {
                string token = System.IO.File.ReadAllText("/var/run/secrets/kubernetes.io/serviceaccount/token");
                _logger.LogInformation($"Getting Kubernetes token: {token}");
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            try
            {
                Models.APIModels.Deployment? apiModel = await httpClient.GetFromJsonAsync<Models.APIModels.Deployment>($"deployments/{name}");

                if (apiModel == null)
                {
                    return NotFound();
                }

                return new Connector
                {
                    DeploymentName = apiModel.metadata?.name ?? "Unknown",
                    Type = apiModel.spec?.template?.spec?.containers.FirstOrDefault()?.name ?? "Unknown",
                    Image = apiModel.spec?.template?.spec?.containers.FirstOrDefault()?.image ?? "Unknown",
                    Replicas = apiModel.status?.replicas ?? 0,
                    AvailableReplicas = apiModel.status?.availableReplicas ?? 0,
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return BadRequest(ex.Message);
            }
        }

        [HttpPut]
        [ProducesResponseType(StatusCodes.Status204NoContent, Type = typeof(Connector))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Put(string name, Connector connector)
        {
            if (name != connector.DeploymentName)
            {
                return BadRequest("name does not equal DeploymentName");
            }

            HttpClient httpClient = _httpClientFactory.CreateClient("kubeClient");
            if (System.IO.File.Exists("/var/run/secrets/kubernetes.io/serviceaccount/token"))
            {
                string token = System.IO.File.ReadAllText("/var/run/secrets/kubernetes.io/serviceaccount/token");
                _logger.LogInformation($"Getting Kubernetes token: {token}");
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            return NoContent();
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(Connector))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Connector>> Post(Connector connector)
        {
            if (connector?.Type == null)
            {
                return BadRequest("Type can not be null");
            }

            ConnectorType? connectorType = await _db.ConnectorType.Include(ct => ct.Images).FirstOrDefaultAsync(ct => ct.Type.ToLower() == connector.Type.ToLower());

            if (connectorType == null)
            {
                ModelState.AddModelError(nameof(connector.Type), "Type not found in database");
            }
            else
            {
                if(connector.Replicas > connectorType.MaxReplicas)
                {
                    ModelState.AddModelError(nameof(connector.Replicas), "Replicas is higher than the max allowed amount for the connector type");
                }

                if(connectorType.Images.Any(ct => ct.Tag.ToLower() == connector.Image?.ToLower()) == false)
                {
                    ModelState.AddModelError(nameof(connector.Image), "Image not found in database");
                }
            }

            if(ModelState.IsValid == false)
            {
                return BadRequest(ModelState);
            }

            string file = await System.IO.File.ReadAllTextAsync("KubernetesTemplates/Template.yaml");

            file = file.Replace("{{name}}", connector.DeploymentName).Replace("{{app}}", connector.Label).Replace("{{image}}", connector.Image).Replace("{{amount}}", connector.Replicas.ToString()).Replace("{{service-name}}", connector.Label + "-service").Replace("{{port}}", "9192");

            HttpClient httpClient = _httpClientFactory.CreateClient("kubeClient");
            if (System.IO.File.Exists("/var/run/secrets/kubernetes.io/serviceaccount/token"))
            {
                string token = System.IO.File.ReadAllText("/var/run/secrets/kubernetes.io/serviceaccount/token");
                _logger.LogInformation($"Getting Kubernetes token: {token}");
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            try
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "deployments");
                request.Content = new StringContent(file, Encoding.UTF8, "application/yaml");

                var response = await httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();
                return CreatedAtAction(nameof(Get), new { name = connector.DeploymentName }, connector);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex.ToString());
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(string name)
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("kubeClient");
            if (System.IO.File.Exists("/var/run/secrets/kubernetes.io/serviceaccount/token"))
            {
                string token = System.IO.File.ReadAllText("/var/run/secrets/kubernetes.io/serviceaccount/token");
                _logger.LogInformation($"Getting Kubernetes token: {token}");
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            try
            {
                var response = await httpClient.DeleteAsync($"deployments/{name}");

                if (response.IsSuccessStatusCode == false)
                {
                    return NotFound();
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("Types")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ConnectorTypeDTO>>> GetTypes()
        {
            _logger.LogInformation("Getting connector types");
            List<ConnectorTypeDTO> connectorTypes = await _db.ConnectorType.Select(i => new ConnectorTypeDTO()
            {
                Type = i.Type,
                Repository = i.Repository,
                MaxReplicas = i.MaxReplicas,
                Images = i.Images.Select(i => i.Tag).ToArray()
            }).ToListAsync();

            return connectorTypes;
        }

        [HttpGet("{name}/Images")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<ImageDTO>>> GetImages(string name)
        {
            _logger.LogInformation("Getting connector images");
            ConnectorType? connectorType = await _db.ConnectorType.Include(ct => ct.Images).FirstOrDefaultAsync(ct => ct.Type.ToLower() == name.ToLower());

            if (connectorType == null)
            {
                return NotFound();
            }

            return connectorType.Images.Select(i => new ImageDTO()
            {
                ConnectorType = i.ConnectorType.Type,
                Tag = i.Tag,
                Digest = i.Digest,
                LastPushed = i.LastPushed
            }).ToList();
        }
    }
}