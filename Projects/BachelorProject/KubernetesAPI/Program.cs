using KubernetesAPI.BackgroundTask;
using KubernetesAPI.Data;
using KubernetesAPI.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();

var saPassword = Environment.GetEnvironmentVariable("MSSQL_SA_PASSWORD");
string? connString = builder.Configuration.GetConnectionString("DefaultConnection");
if (saPassword != null)
{
    string[] values = connString.Split(";");
    string[] server = values[0].Split(":");
    values[0] = server[0] + ":mssql-service";
    string[] pass = values[3].Split("=");
    values[3] = pass[0] + $"={saPassword}";
    connString = string.Join(";", values);
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        connString, s =>
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
        if (port == "443")
        {
            options.BaseAddress = new Uri($"https://{host}:{port}/apis/apps/v1/namespaces/default/");
        }
        else
        {
            options.BaseAddress = new Uri($"http://{host}:{port}/apis/apps/v1/namespaces/default/");
        }
    }).ConfigurePrimaryHttpMessageHandler(mh =>
    {
        var handler = new HttpClientHandler();
        handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };
        return handler;
    });

    builder.Services.AddHttpClient("kubeCoreClient", options =>
    {
        if (port == "443")
        {
            options.BaseAddress = new Uri($"https://{host}:{port}/api/v1/namespaces/default/");
        }
        else
        {
            options.BaseAddress = new Uri($"http://{host}:{port}/api/v1/namespaces/default/");
        }
    }).ConfigurePrimaryHttpMessageHandler(mh =>
    {
        var handler = new HttpClientHandler();
        handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };
        return handler;
    });
}
else
{
    builder.Services.AddHttpClient("kubeClient", options =>
    {
        options.BaseAddress = new Uri(builder.Configuration["Kubernetes:APIURL"] ?? throw new ArgumentNullException("Kubernetes:APIURL"));
    });

    builder.Services.AddHttpClient("kubeCoreClient", options =>
    {
        options.BaseAddress = new Uri(builder.Configuration["Kubernetes:CoreAPIURL"] ?? throw new ArgumentNullException("Kubernetes:APIURL"));
    });
}

builder.Services.AddHostedService<UpdateImages>();
builder.Services.Configure<KubernetesOptions>(builder.Configuration.GetSection("Kubernetes"));
builder.Services.Configure<DockerOptions>(builder.Configuration.GetSection("Docker"));

builder.Services.AddHealthChecks();

var app = builder.Build();

app.MapHealthChecks("/health");

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
