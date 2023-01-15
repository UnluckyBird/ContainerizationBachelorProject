using KubernetesAPI.Models;
using KubernetesAPI.Settings;
using Microsoft.AspNetCore.Mvc;
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
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IOptionsMonitor<Appsettings> _appsettings;

        public ConnectorController(ILogger<ConnectorController> logger, IHttpClientFactory httpClientFactory, IOptionsMonitor<Appsettings> appsettings)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _appsettings = appsettings;
        }

        /*[HttpGet("GetTypes")]
        public async Task<ActionResult<IEnumerable<string>>> GetTypes()
        {
            string? imagesFolder = _appsettings.CurrentValue.ImagesFolder;
            if (imagesFolder == null)
            {
                return NotFound();
            }

            return Directory.GetDirectories(imagesFolder).Select(d => new DirectoryInfo(d).Name).ToList();
        }*/

        [HttpGet("GetImages")]
        public async Task<ActionResult<IEnumerable<Image>>> GetImages()
        {
            Process p = new Process();
            
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.FileName = "cmd.exe";
            p.StartInfo.Arguments = "/C docker images --all";
            p.Start();

            string output = p.StandardOutput.ReadToEnd();
            p.WaitForExit();

            List<Image> images = new List<Image>();
            string[] lines = output.Split("\n");
            for(int i = 1; i< lines.Length - 1; i++)
            {
                string[] image = lines[i].Split(" ", StringSplitOptions.RemoveEmptyEntries);
                images.Add(new Image() {
                    Repository = image[0],
                    Tag = image[1],
                    Id = image[2],
                    Created = $"{image[3]} {image[4]} {image[5]}",
                    Size = image[6]
                });
            }
            return images;
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
    }
}