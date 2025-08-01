using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
// using OpenTelemetry.Metrics;
// using OpenTelemetry.Resources;
// using OpenTelemetry.Trace;
using PerformanceApi.Data;
using PerformanceApi.Repositories;
using PerformanceApi.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. Configuração de Serviços e Injeção de Dependência
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configurando o EF Core com a connection string do appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<PerformanceDbContext>(options =>
    //options.UseLazyLoadingProxies()
    options.UseSqlServer(connectionString));

// Connection factory
builder.Services.AddSingleton<IDbConnectionFactory>(
    new SqlServerConnectionFactory(builder.Configuration.GetConnectionString("DefaultConnection")));

// Injetando as camadas
builder.Services.AddScoped<IStatementService, StatementService>();
//builder.Services.AddScoped<IStatementRepository, StatementRepository>();
builder.Services.AddScoped<IStatementRepository, StatementDapperRepository>();

// #######################################################################
// ### 2. CONFIGURAÇÃO DO OPENTELEMETRY                                ###
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
        // .AddOtlpExporter(opt => // Envia os dados para um Coletor OpenTelemetry
        // {
        //     // O endpoint padrão do coletor OTLP. O Grafana Agent ou outro coletor
        //     // estará ouvindo nesta porta.
        //     opt.Endpoint = new Uri("http://localhost:4318");
        //     opt.Protocol = OtlpExportProtocol.HttpProtobuf;
        .AddJaegerExporter(opt =>
        {
            opt.Endpoint = new Uri("http://localhost:14268/api/traces");
            opt.Protocol = JaegerExportProtocol.HttpBinaryThrift;
        }))
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddRuntimeInstrumentation()
        // .AddOtlpExporter(opt => // Envia os dados para um Coletor OpenTelemetry
        // {
        //     opt.Endpoint = new Uri("http://localhost:4318");
        //     opt.Protocol = OtlpExportProtocol.HttpProtobuf;
        // }));
        .AddPrometheusExporter());

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseOpenTelemetryPrometheusScrapingEndpoint();
app.UseAuthorization();
app.MapControllers();
app.Run();