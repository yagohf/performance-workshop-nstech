using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using PerformanceApi.Data;
using PerformanceApi.Repositories;
using PerformanceApi.Services;

var builder = WebApplication.CreateBuilder(args);

// #######################################################################
// 1. Middlewares do ASP.NET e Swagger
// #######################################################################

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// #######################################################################
// 2. Configurando acesso a banco
// #######################################################################

// 2.1. EF Core
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<PerformanceDbContext>(options =>
    options.UseLazyLoadingProxies()
    .UseSqlServer(connectionString));

builder.Services.AddScoped<IStatementRepository, StatementRepository>();

// #######################################################################
// 3. Serviços
// #######################################################################

builder.Services.AddScoped<IRiskScoreService, RiskScoreService>();
builder.Services.AddScoped<IStatementService, StatementService>();

// #######################################################################
// 4. OpenTelemetry
// #######################################################################
const string serviceName = "PerformanceApi.Demo";
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService(serviceName))
    .WithTracing(tracing => tracing
        .AddSource(serviceName)
        .SetSampler(new AlwaysOnSampler())
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddSqlClientInstrumentation(options => // Essencial para ver as queries
        {
            options.SetDbStatementForText = true;
            options.RecordException = true;
        })
        .AddJaegerExporter(opt =>
        {
            opt.Endpoint = new Uri("http://localhost:14268/api/traces");
            opt.Protocol = JaegerExportProtocol.HttpBinaryThrift;
        }))
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddRuntimeInstrumentation()
        .AddPrometheusExporter());

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// OpenTelemetry
app.UseOpenTelemetryPrometheusScrapingEndpoint();

app.MapControllers();
app.Run();