using KubernetesAPI.Data;

namespace KubernetesAPI.BackgroundTask
{
    public class UpdateImages : IHostedService, IDisposable
    {
        private readonly ILogger _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private Timer _timer;

        public UpdateImages(ILogger<UpdateImages> logger,
            IServiceScopeFactory scopeFactory
            )
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
        }
        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Timed hosted service is starting.");

            _timer = new Timer(DoWork, null, TimeSpan.Zero,
                TimeSpan.FromMinutes(5));

            return Task.CompletedTask;
        }

        private async void DoWork(object state)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                ApplicationDbContext _db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                try
                {
                          
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in hosted service: {ex.Message}");
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
    }
}
