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
using KubernetesAPI.Models.DTO.Post;
using KubernetesAPI.Models.DTO.Patch;
using System.Text.Json;
using KubernetesAPI.Models.DTO.Put;

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
        private readonly IOptionsMonitor<DockerOptions> _dockerOptionsMonitor;


        public ConnectorController(ILogger<ConnectorController> logger, ApplicationDbContext db, IHttpClientFactory httpClientFactory, IOptionsMonitor<KubernetesOptions> kubernetesOptionsMonitor, IOptionsMonitor<DockerOptions> dockerOptionsMonitor)
        {
            _logger = logger;
            _db = db;
            _httpClientFactory = httpClientFactory;
            _kubernetesOptionsMonitor = kubernetesOptionsMonitor;
            _dockerOptionsMonitor = dockerOptionsMonitor;
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
                    DeploymentTime = d.metadata?.creationTimestamp ?? DateTime.MinValue,
                    Type = d.spec?.template?.spec?.containers.FirstOrDefault()?.image?.Substring(0, d.spec.template.spec.containers.First().image.LastIndexOf('-')) ?? "Unknown",
                    Image = d.spec?.template?.spec?.containers.FirstOrDefault()?.image ?? "Unknown",
                    Replicas = d.status?.replicas ?? 0,
                    AvailableReplicas = d.status?.availableReplicas ?? 0,
                    Env = d.spec?.template?.spec?.containers.FirstOrDefault()?.env?.Select(e => new EnvironmentVariable
                    {
                        Name = e.name,
                        Value = e.valueFrom == null ? e.value : e.valueFrom.secretKeyRef != null ? "****" : e.valueFrom.configMapKeyRef?.name != null ? $"in configmap {e.valueFrom.configMapKeyRef.name}" : "Unknown"
                    }).ToList() ?? new List<EnvironmentVariable>()
                }).ToArray();
            }
            catch(Exception ex)
            {
                _logger.LogError(ex.ToString());
                return BadRequest(ex.Message);
            }         
        }

        [HttpGet("{deploymentName}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Connector))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Connector>> Get(string deploymentName)
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("kubeClient");
            if (System.IO.File.Exists("/var/run/secrets/kubernetes.io/serviceaccount/token"))
            {
                string token = System.IO.File.ReadAllText("/var/run/secrets/kubernetes.io/serviceaccount/token");
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            try
            {
                Models.APIModels.Deployment? apiModel = await httpClient.GetFromJsonAsync<Models.APIModels.Deployment>($"deployments/{deploymentName}");

                if (apiModel == null)
                {
                    return NotFound();
                }

                return new Connector
                {
                    DeploymentName = apiModel.metadata?.name ?? "Unknown",
                    DeploymentTime = apiModel.metadata?.creationTimestamp ?? DateTime.MinValue,
                    Type = apiModel.spec?.template?.spec?.containers.FirstOrDefault()?.image?.Substring(0, apiModel.spec.template.spec.containers.First().image.LastIndexOf('-')) ?? "Unknown",
                    Image = apiModel.spec?.template?.spec?.containers.FirstOrDefault()?.image ?? "Unknown",
                    Replicas = apiModel.status?.replicas ?? 0,
                    AvailableReplicas = apiModel.status?.availableReplicas ?? 0,
                    Env = apiModel.spec?.template?.spec?.containers.FirstOrDefault()?.env?.Select(e => new EnvironmentVariable
                    {
                        Name = e.name,
                        Value = e.valueFrom == null ? e.value : e.valueFrom.secretKeyRef != null ? "****" : e.valueFrom.configMapKeyRef?.name != null ? $"in configmap {e.valueFrom.configMapKeyRef.name}" : "Unknown"
                    }).ToList() ?? new List<EnvironmentVariable>()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return BadRequest(ex.Message);
            }
        }

        [HttpPatch]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Patch(string deploymentName, ConnectorPatchDTO connector)
        {
            if (deploymentName != connector.DeploymentName)
            {
                return BadRequest("You can not change the deployment name");
            }

            HttpClient httpClient = _httpClientFactory.CreateClient("kubeClient");
            if (System.IO.File.Exists("/var/run/secrets/kubernetes.io/serviceaccount/token"))
            {
                string token = System.IO.File.ReadAllText("/var/run/secrets/kubernetes.io/serviceaccount/token");
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            DockerOptions dockerOptions = _dockerOptionsMonitor.CurrentValue;

            var json = """
                {
                    "spec": {
                        "replicas":"replicasAmount",
                        "template": {
                            "spec": {
                                "containers": [
                                {
                                    "name": "namePlaceholder",
                                    "image": "imagePlaceholder"
                                }
                                ]
                            }
                        }
                    }
                }
            """.Replace("\"replicasAmount\"", connector.Replicas.ToString())
            .Replace("namePlaceholder", connector.DeploymentName)
            .Replace("imagePlaceholder", $"{dockerOptions.Namespace}/{dockerOptions.Repository}:{connector.Image}");

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Patch, $"deployments/{deploymentName}");
            request.Content = new StringContent(json, Encoding.UTF8, "application/strategic-merge-patch+json");

            var response = await httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            return NoContent();
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(Connector))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Connector>> Post(ConnectorPostDTO connector)
        {
            if (connector?.Type == null)
            {
                ModelState.AddModelError(nameof(connector.Type), "Type can not be null");
            }

            if (connector.Type != connector.Image.Substring(0, connector.Image.LastIndexOf('-')))
            {
                ModelState.AddModelError(nameof(connector.Image), $"Image is not type {connector.Type}");
            }

            if (connector.CreateService == true)
            {
                if (connector.ExternalPortNumber == null)
                {
                    ModelState.AddModelError(nameof(connector.ExternalPortNumber), $"PortNumber cannot be null, when creating service");
                }
                else if (connector.ExternalPortNumber < 30000)
                {
                    ModelState.AddModelError(nameof(connector.ExternalPortNumber), $"PortNumber must be atleast 30000");
                }
            }

            if (ModelState.IsValid == false)
            {
                return BadRequest(ModelState);
            }

            ConnectorType? connectorType = await _db.ConnectorType.Include(ct => ct.Images).Include(ct => ct.ExposedPorts).FirstOrDefaultAsync(ct => ct.Type.ToLower() == connector.Type.ToLower());

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

            HttpClient httpClient = _httpClientFactory.CreateClient("kubeClient");
            if (System.IO.File.Exists("/var/run/secrets/kubernetes.io/serviceaccount/token"))
            {
                string token = System.IO.File.ReadAllText("/var/run/secrets/kubernetes.io/serviceaccount/token");
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            try
            {
                DockerOptions dockerOptions = _dockerOptionsMonitor.CurrentValue;

                string deployFile = await System.IO.File.ReadAllTextAsync("KubernetesTemplates/DeploymentTemplate.yaml");
                deployFile = deployFile.Replace("{{name}}", connector.DeploymentName).Replace("{{app}}", connector.DeploymentName).Replace("{{image}}", $"{dockerOptions.Namespace}/{dockerOptions.Repository}:{connector.Image}").Replace("{{amount}}", connector.Replicas.ToString()).Replace("{{port}}", connectorType.ExposedPorts.First().Port.ToString());

                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "deployments");
                request.Content = new StringContent(deployFile, Encoding.UTF8, "application/yaml");

                var response = await httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();
                if(connector.CreateService == true)
                {
                    HttpClient httpClientCore = _httpClientFactory.CreateClient("kubeCoreClient");
                    if (System.IO.File.Exists("/var/run/secrets/kubernetes.io/serviceaccount/token"))
                    {
                        string token = System.IO.File.ReadAllText("/var/run/secrets/kubernetes.io/serviceaccount/token");
                        httpClientCore.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    }

                    string serviceFile = await System.IO.File.ReadAllTextAsync("KubernetesTemplates/ServiceTemplate.yaml");
                    serviceFile = serviceFile.Replace("{{app}}", connector.DeploymentName)
                        .Replace("{{service-name}}", connector.DeploymentName + "-service")
                        .Replace("{{port}}", connectorType.ExposedPorts.First().Port.ToString())
                        .Replace("{{external-port}}", connector.ExternalPortNumber.ToString());

                    HttpRequestMessage serviceRequest = new HttpRequestMessage(HttpMethod.Post, "services");
                    serviceRequest.Content = new StringContent(serviceFile, Encoding.UTF8, "application/yaml");

                    var serviceResponse = await httpClientCore.SendAsync(serviceRequest);
                    serviceResponse.EnsureSuccessStatusCode();
                }

                return CreatedAtAction(nameof(Get), new { name = connector.Type }, connector);
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
        public async Task<IActionResult> Delete(string deploymentName)
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("kubeClient");
            if (System.IO.File.Exists("/var/run/secrets/kubernetes.io/serviceaccount/token"))
            {
                string token = System.IO.File.ReadAllText("/var/run/secrets/kubernetes.io/serviceaccount/token");
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            try
            {
                var response = await httpClient.DeleteAsync($"deployments/{deploymentName}");

                if (response.IsSuccessStatusCode == false)
                {
                    return NotFound();
                }

                HttpClient httpClientCore = _httpClientFactory.CreateClient("kubeCoreClient");
                if (System.IO.File.Exists("/var/run/secrets/kubernetes.io/serviceaccount/token"))
                {
                    string token = System.IO.File.ReadAllText("/var/run/secrets/kubernetes.io/serviceaccount/token");
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }
                await httpClientCore.DeleteAsync($"services/{deploymentName}-service");

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
                Images = i.Images.Select(i => i.Tag).ToArray(),
                ExposedPorts = i.ExposedPorts.Select(i => i.Port).ToArray()
            }).ToListAsync();

            return connectorTypes;
        }

        [HttpPut("Types")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ConnectorTypeDTO>> PutTypes(string type, ConnectorTypePutDTO connectorType)
        {
            _logger.LogInformation("Updating connector type");

            if (type != connectorType.Type)
            {
                return BadRequest("You can not change the connector type");
            }

            ConnectorType? dbConnectorTypes = await _db.ConnectorType.Include(ct => ct.ExposedPorts).FirstOrDefaultAsync(i => i.Type.ToLower() == type.ToLower());

            if (dbConnectorTypes == null)
            {
                return BadRequest($"Connector type of {type} not found");
            }

            dbConnectorTypes.Repository = connectorType.Repository;
            dbConnectorTypes.MaxReplicas = connectorType.MaxReplicas;
            dbConnectorTypes.ExposedPorts = connectorType.ExposedPorts.Select(ep => new ExposedPort
            {
                Port = ep
            }).ToList();
            _db.Update(dbConnectorTypes);
            await _db.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("{connectorType}/Images")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<ImageDTO>>> GetImages(string connectorType)
        {
            _logger.LogInformation("Getting connector images");
            ConnectorType? DBConnectorType = await _db.ConnectorType.Include(ct => ct.Images).FirstOrDefaultAsync(ct => ct.Type.ToLower() == connectorType.ToLower());

            if (DBConnectorType == null)
            {
                return NotFound();
            }

            return DBConnectorType.Images.Select(i => new ImageDTO()
            {
                ConnectorType = i.ConnectorType.Type ?? "Unknown",
                Tag = i.Tag,
                Digest = i.Digest,
                LastPushed = i.LastPushed
            }).ToList();
        }

        [HttpGet("{deploymentName}/Pods")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<PodDTO>>> GetPods(string deploymentName)
        {
            _logger.LogInformation("Getting connector pods");

            HttpClient httpClient = _httpClientFactory.CreateClient("kubeCoreClient");
            if (System.IO.File.Exists("/var/run/secrets/kubernetes.io/serviceaccount/token"))
            {
                string token = System.IO.File.ReadAllText("/var/run/secrets/kubernetes.io/serviceaccount/token");
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            try
            {
                Models.APIModels.Pods? apiModel = await httpClient.GetFromJsonAsync<Models.APIModels.Pods>($"pods?labelSelector=app%3D{deploymentName}");

                if (apiModel == null)
                {
                    return NotFound();
                }

                return apiModel.items.Select(d => new PodDTO
                {
                    Name = d.metadata?.name ?? "Unknown",
                }).ToArray();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return BadRequest(ex.Message);
            }
        }
    }
}