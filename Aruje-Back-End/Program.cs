using System.Reflection;
using Aruje.Application.DependencyInjection;
using Aruje.Infrastructure.DependencyInjection;
using Microsoft.OpenApi.Models;
using Aruje_Back_End.Middlewares;

var builder = WebApplication.CreateBuilder(args);

// Controllers
builder.Services.AddControllers();

// Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Arujé API",
        Version = "v1",
        Description = "API para monitoramento agrícola inteligente com IoT, IA e arquitetura limpa.",
        Contact = new OpenApiContact
        {
            Name = "Gustavo Gomes",
            Email = "gustavogomesmartins4@icloud.com"
        }
    });

    options.EnableAnnotations();

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

// Application / Infrastructure
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// Pipeline HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Arujé API v1");
        options.DocumentTitle = "Arujé API Documentation";
    });
}

app.UseMiddleware<ErrorHandlingMiddleware>();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();