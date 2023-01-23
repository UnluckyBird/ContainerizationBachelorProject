using KubernetesAPI.BackgroundTask;
using KubernetesAPI.Data;
using KubernetesAPI.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.OpenApi.Models;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
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
    options.BaseAddress = new Uri(builder.Configuration["Docker:APIURL"] ?? "");
});

builder.Services.AddHttpClient("kubeClient" ,options =>
{
    options.BaseAddress = new Uri(builder.Configuration["App:KubernetesAPIURL"] ?? "");
});

builder.Services.AddHostedService<UpdateImages>();
builder.Services.Configure<Appsettings>(builder.Configuration.GetSection("App"));
builder.Services.Configure<DockerOptions>(builder.Configuration.GetSection("Docker"));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
