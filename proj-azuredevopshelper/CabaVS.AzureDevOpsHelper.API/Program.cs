using System.Globalization;
using Azure.Monitor.OpenTelemetry.Exporter;
using CabaVS.AzureDevOpsHelper.Application;
using CabaVS.AzureDevOpsHelper.Infrastructure;
using CabaVS.AzureDevOpsHelper.Persistence;
using CabaVS.AzureDevOpsHelper.Presentation;
using Microsoft.ApplicationInsights;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.OpenTelemetry;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Server
builder.WebHost.ConfigureKestrel(
    options => options.AddServerHeader = false);

// Logging
builder.Host.UseSerilog((context, services, configuration) =>
{
    _ = configuration
        .MinimumLevel.Warning()
        .MinimumLevel.Override("CabaVS", LogEventLevel.Information)
        .Enrich.FromLogContext()
        .Enrich.WithMachineName()
        .Enrich.WithThreadId();
    
    _ = context.HostingEnvironment.IsDevelopment()
        ? configuration
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}",
                formatProvider: CultureInfo.InvariantCulture)
            .WriteTo.OpenTelemetry(options => options.Protocol = OtlpProtocol.Grpc)
        : configuration.WriteTo.ApplicationInsights(
            services.GetRequiredService<TelemetryClient>(),
            TelemetryConverter.Traces);
});

// Open Telemetry
builder.Services.AddOpenTelemetry()
    .ConfigureResource(_ => ResourceBuilder.CreateDefault())
    .WithMetrics(metrics =>
    {
        MeterProviderBuilder meterProviderBuilder = metrics
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddRuntimeInstrumentation();
        _ = builder.Environment.IsDevelopment()
            ? meterProviderBuilder.AddOtlpExporter()
            : meterProviderBuilder.AddAzureMonitorMetricExporter();
    })
    .WithTracing(tracing =>
    {
        TracerProviderBuilder tracerProviderBuilder = tracing
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation();
        _ = builder.Environment.IsDevelopment()
            ? tracerProviderBuilder.AddOtlpExporter()
            : tracerProviderBuilder.AddAzureMonitorTraceExporter();
    });

// Services
builder.Services
    .AddApplication()
    .AddPersistence(builder.Configuration)
    .AddInfrastructure(builder.Configuration)
    .AddPresentation(builder.Environment.IsDevelopment());

WebApplication app = builder.Build();

app.UsePresentation(app.Environment.IsDevelopment());

await app.RunAsync();
