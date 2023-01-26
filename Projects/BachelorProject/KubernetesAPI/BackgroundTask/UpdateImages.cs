using KubernetesAPI.Data;
using KubernetesAPI.Models.APIModels;
using KubernetesAPI.Models.DBModels;
using KubernetesAPI.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using static System.Net.WebRequestMethods;

namespace KubernetesAPI.BackgroundTask
{
    public class UpdateImages : IHostedService, IDisposable
    {
        private readonly ILogger _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private Timer _timer;
        private string _token;

        public UpdateImages(ILogger<UpdateImages> logger,
            IServiceScopeFactory scopeFactory
            )
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
        }
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Timed hosted service is starting.");

            await RefreshBearerToken();

            _timer = new Timer(DoWork, null, TimeSpan.Zero,
                TimeSpan.FromMinutes(5));
        }

        private async void DoWork(object state)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                ApplicationDbContext _db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                IHttpClientFactory _httpClientFactory = scope.ServiceProvider.GetRequiredService<IHttpClientFactory>();
                IOptionsMonitor<DockerOptions> _dockerOptionsMonitor = scope.ServiceProvider.GetRequiredService<IOptionsMonitor<DockerOptions>>();

                DockerOptions _dockerOptions = _dockerOptionsMonitor.CurrentValue;
                HttpClient httpClient = _httpClientFactory.CreateClient("dockerHubClient");
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);

                try
                {
                    DockerImages? response = await httpClient.GetFromJsonAsync<DockerImages>($"namespaces/{_dockerOptions.Namespace}/repositories/{_dockerOptions.Repository}/images?status=active&currently_tagged=true&page_size=50");
                    
                    if (response != null)
                    {
                        response.Results.ForEach(r => { r.Tags = r.Tags.Where(r => r.IsCurrent).ToList(); });
                        List<ConnectorType> connectorTypes = await _db.ConnectorType.Include(ct => ct.Images).ToListAsync();
                        foreach (Result result in response.Results)
                        {
                            string imageType = result.Tags.First().TagName;
                            imageType = imageType.Substring(0, imageType.LastIndexOf("-"));
                            ConnectorType? connectorType = connectorTypes.FirstOrDefault(ct => ct.Type == imageType);
                            if (connectorType == null)
                            {
                                connectorType = new ConnectorType() 
                                { 
                                    Type = imageType,
                                    Repository = $"{result.NameSpace}/{result.Repository}",
                                    Images = new List<Image>()
                                };
                                connectorTypes.Add(connectorType);
                            }

                            foreach(Tag tag in result.Tags)
                            {
                                Image? image = connectorType.Images.FirstOrDefault(i => i.Tag == tag.TagName);
                                if(image != null)
                                {
                                    image.Digest = result.Digest;
                                    image.LastPushed = result.LastPushed;
                                }
                                else
                                {
                                    connectorType.Images.Add(new Image()
                                    {
                                        Tag = tag.TagName,
                                        Digest = result.Digest,
                                        LastPushed = result.LastPushed
                                    });
                                }
                            }
                        }
                        _db.ConnectorType.UpdateRange(connectorTypes);
                        await _db.SaveChangesAsync();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.ToString());
                    await RefreshBearerToken();
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Timed hosted service is stopping.");

            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }

        private async Task RefreshBearerToken()
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                IHttpClientFactory _httpClientFactory = scope.ServiceProvider.GetRequiredService<IHttpClientFactory>();
                IOptionsMonitor<DockerOptions> _dockerOptionsMonitor = scope.ServiceProvider.GetRequiredService<IOptionsMonitor<DockerOptions>>();

                DockerOptions _dockerOptions = _dockerOptionsMonitor.CurrentValue;
                HttpClient httpClient = _httpClientFactory.CreateClient("dockerHubClient");
                try
                {
                    var body = new
                    {
                        Username = _dockerOptions.Username,
                        Password = _dockerOptions.Password
                    };
                    var response = await httpClient.PostAsJsonAsync("users/login", body);
                    DockerToken? token = await response.Content.ReadFromJsonAsync<DockerToken>();

                    if(token != null)
                    {
                        _token = token.Token;
                    }
                    else
                    {
                        _logger.LogError($"Error refreshing docker api oauth token statuscode: {response.StatusCode}");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error refreshing docker api oauth token: {ex.Message}");
                }
            }
        }
    }
}
