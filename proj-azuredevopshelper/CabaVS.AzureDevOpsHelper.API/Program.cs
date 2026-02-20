using System.Globalization;
using Azure.Monitor.OpenTelemetry.Exporter;
using CabaVS.AzureDevOpsHelper.API.Configuration;
using CabaVS.AzureDevOpsHelper.API.Services;
using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Options;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
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

// Configuration
builder.Services.Configure<AzureDevOpsOptions>(
    builder.Configuration.GetSection(AzureDevOpsOptions.SectionName));
builder.Services.Configure<TeamDefinitionOptions>(
    builder.Configuration.GetSection(TeamDefinitionOptions.SectionName));

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
builder.Services.AddSingleton(sp =>
{
    AzureDevOpsOptions options = sp.GetRequiredService<IOptions<AzureDevOpsOptions>>().Value;

    var credentials = new VssBasicCredential(string.Empty, options.Pat);
    var connection = new VssConnection(new Uri(options.OrgUrl), credentials);

    return connection.GetClient<WorkItemTrackingHttpClient>();
});

builder.Services.AddSingleton<IAzureDevOpsService, AzureDevOpsService>();

WebApplication app = builder.Build();

app.MapGet("/api/work-items/{workItemId:int}/remaining-work",
    async (
        int workItemId,
        IAzureDevOpsService azureDevOpsService,
        CancellationToken cancellationToken) =>
{
    Dictionary<string, Dictionary<string, double>> report = await azureDevOpsService.BuildRemainingWorkReportByTeamAndActivityType(workItemId, cancellationToken);

    return Results.Ok(report);
});

await app.RunAsync();
