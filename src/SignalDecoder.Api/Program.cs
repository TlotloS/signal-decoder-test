using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using SignalDecoder.Api.Filters;
using SignalDecoder.Application.Services;
using SignalDecoder.Domain.Interfaces;
using Swashbuckle.AspNetCore.Filters;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Use camelCase for JSON serialization
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

// Register application services
builder.Services.AddScoped<ISignalDecoderService, SignalDecoderService>();
builder.Services.AddScoped<IDeviceGeneratorService, DeviceGeneratorService>();
builder.Services.AddScoped<ISignalSimulatorService, SignalSimulatorService>();

// Configure Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Signal Decoder API",
        Version = "v1",
        Description = "API for device signal generation, simulation, and decoding. " +
                      "Use the examples below to understand the signal transmission workflow:\n\n" +
                      "1. **Generate Devices**: Create random devices with signal patterns\n" +
                      "2. **Simulate Transmission**: Randomly select active devices and compute combined signal\n" +
                      "3. **Decode Signal**: Identify which device combinations match the received signal"
    });

    // Include XML comments for better Swagger documentation
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }

    // Enable examples from IExamplesProvider implementations
    options.ExampleFilters();

    // Apply custom schema filter for better array display
    options.SchemaFilter<ArrayDisplaySchemaFilter>();
});

// Configure Swagger to use compact JSON serialization
builder.Services.ConfigureSwaggerGen(options =>
{
    options.CustomSchemaIds(type => type.FullName);
});

// Register example providers for Swagger
builder.Services.AddSwaggerExamplesFromAssemblyOf<Program>();

// Configure CORS (allow all origins for local development)
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger(options =>
    {
        options.SerializeAsV2 = false;
    });
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Signal Decoder API v1");
        options.DefaultModelsExpandDepth(2);
        options.DefaultModelExpandDepth(2);
        options.DisplayRequestDuration();
    });
}

// Global exception handler middleware
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync("{\"error\": \"An unexpected error occurred.\"}");
    });
});

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthorization();
app.MapControllers();

app.Run();

// Make Program class accessible for integration tests
public partial class Program { }
