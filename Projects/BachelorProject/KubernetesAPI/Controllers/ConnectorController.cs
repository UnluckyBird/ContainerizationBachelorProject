using KubernetesAPI.Data;
using KubernetesAPI.Models;
using KubernetesAPI.Models.DBModels;
using KubernetesAPI.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Diagnostics;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Text;

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
                return BadRequest(ex.Message);
            }
        }

        [HttpPut]
        [ProducesResponseType(StatusCodes.Status204NoContent, Type = typeof(Connector))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Put(string name, Connector connector)
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("kubeClient");
            if (name != connector.DeploymentName)
            {
                return BadRequest("name does not equal DeploymentName");
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

                if(connectorType.Images.Any(ct => ct.Name.ToLower() == connector.Image?.ToLower()) == false)
                {
                    ModelState.AddModelError(nameof(connector.Image), "Image not found in database");
                }
            }

            if(ModelState.IsValid == false)
            {
                return BadRequest(ModelState);
            }

            KubernetesOptions options = _kubernetesOptionsMonitor.CurrentValue;
            string file = await System.IO.File.ReadAllTextAsync(options.TemplateFolder + "Template.yaml");

            file = file.Replace("{{name}}", connector.DeploymentName).Replace("{{app}}", connector.Label).Replace("{{image}}", connector.Image).Replace("{{amount}}", connector.Replicas.ToString()).Replace("{{service-name}}", connector.Label + "-service").Replace("{{port}}", "9192");

            HttpClient httpClient = _httpClientFactory.CreateClient("kubeClient");
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "deployments");
            request.Content = new StringContent(file, Encoding.UTF8, "application/yaml");

            var response = await httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            return CreatedAtAction(nameof(Get), new { name = connector.DeploymentName }, connector);
        }

        [HttpDelete]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(string name)
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("kubeClient");
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
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("Types")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ConnectorType>>> GetTypes()
        {
            List<ConnectorType> connectorTypes = await _db.ConnectorType.ToListAsync();

            return connectorTypes;
        }

        [HttpGet("{name}/Images")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<Image>>> GetImages(string name)
        {
            ConnectorType? connectorType = await _db.ConnectorType.Include(ct => ct.Images).FirstOrDefaultAsync(ct => ct.Type.ToLower() == name);

            if (connectorType == null)
            {
                return NotFound();
            }

            return connectorType.Images.ToList();
        }
    }
}