using KubernetesAPI.BackgroundTask;
using KubernetesAPI.Data;
using KubernetesAPI.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.OpenApi.Models;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"), s =>
        {
            s.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
        }
    )
);
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "Kubernetes API",
        Description = "An API for managing kubernetes deployments",
    });
});

builder.Services.AddHttpClient("dockerHubClient", options =>
{
    options.BaseAddress = new Uri(builder.Configuration["Docker:APIURL"] ?? throw new ArgumentNullException("Docker:APIURL"));
});

var host = Environment.GetEnvironmentVariable("KUBERNETES_SERVICE_HOST");
var port = Environment.GetEnvironmentVariable("KUBERNETES_SERVICE_PORT_HTTPS");

if (host != null && port != null)
{
    builder.Services.AddHttpClient("kubeClient", options =>
    {
        options.BaseAddress = new Uri($"https://{host}:{port}/apis/apps/v1/namespaces/default/");
    });
}
else
{
    builder.Services.AddHttpClient("kubeClient", options =>
    {
        options.BaseAddress = new Uri(builder.Configuration["Kubernetes:APIURL"] ?? throw new ArgumentNullException("Kubernetes:APIURL"));
    });
}


builder.Services.AddHostedService<UpdateImages>();
builder.Services.Configure<KubernetesOptions>(builder.Configuration.GetSection("Kubernetes"));
builder.Services.Configure<DockerOptions>(builder.Configuration.GetSection("Docker"));

builder.Services.AddHealthChecks();

var app = builder.Build();

app.MapHealthChecks("/health");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
