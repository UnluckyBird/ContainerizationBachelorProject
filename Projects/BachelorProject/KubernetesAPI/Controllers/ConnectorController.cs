using KubernetesAPI.Data;
using KubernetesAPI.Models;
using KubernetesAPI.Models.DBModels;
using KubernetesAPI.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Diagnostics;
using System.Reflection;

namespace KubernetesAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ConnectorController : ControllerBase
    {
        private readonly ILogger<ConnectorController> _logger;
        private readonly ApplicationDbContext _db;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IOptionsMonitor<Appsettings> _appsettings;

        public ConnectorController(ILogger<ConnectorController> logger, ApplicationDbContext db, IHttpClientFactory httpClientFactory, IOptionsMonitor<Appsettings> appsettings)
        {
            _logger = logger;
            _db = db;
            _httpClientFactory = httpClientFactory;
            _appsettings = appsettings;
        }

        [HttpGet]
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
                    Name = d.metadata?.name ?? "Unknown",
                    Type = d.spec?.template?.spec?.containers.FirstOrDefault()?.name ?? "Unknown",
                    Image = d.spec?.template?.spec?.containers.FirstOrDefault()?.image ?? "Unknown",
                    Replicas = d.status?.replicas ?? 0,
                    AvaiableReplicas = d.status?.availableReplicas ?? 0,
                }).ToArray();
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }         
        }

        [HttpGet("{name}")]
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
                    Name = apiModel.metadata?.name ?? "Unknown",
                    Type = apiModel.spec?.template?.spec?.containers.FirstOrDefault()?.name ?? "Unknown",
                    Image = apiModel.spec?.template?.spec?.containers.FirstOrDefault()?.image ?? "Unknown",
                    Replicas = apiModel.status?.replicas ?? 0,
                    AvaiableReplicas = apiModel.status?.availableReplicas ?? 0,
                };
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Put(string name, Connector connector)
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("kubeClient");
            if (name != connector.Name)
            {
                return BadRequest();
            }
            return NoContent();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult<Connector>> Post(Connector connector)
        {
            if (connector == null)
            {
                return BadRequest();
            }
            HttpClient httpClient = _httpClientFactory.CreateClient("kubeClient");
            return CreatedAtAction(nameof(Get), new { name = connector.Name }, connector);
        }

        [HttpDelete]
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
        public async Task<ActionResult<IEnumerable<ConnectorType>>> GetTypes()
        {
            List<ConnectorType> connectorTypes = await _db.ConnectorType.ToListAsync();

            return connectorTypes;
        }

        [HttpGet("{name}/Images")]
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