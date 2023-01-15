using KubernetesAPI.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
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

builder.Services.AddHttpClient("kubeClient" ,options =>
{
    options.BaseAddress = new Uri(builder.Configuration["App:KubernetesAPIURL"] ?? "");
});

builder.Services.Configure<Appsettings>(builder.Configuration.GetSection("App"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
