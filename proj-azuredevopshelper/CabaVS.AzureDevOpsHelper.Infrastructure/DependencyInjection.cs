using CabaVS.AzureDevOpsHelper.Application.Contracts.Infrastructure;
using CabaVS.AzureDevOpsHelper.Infrastructure.Configuration;
using CabaVS.AzureDevOpsHelper.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;

namespace CabaVS.AzureDevOpsHelper.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        _ = services.Configure<AzureDevOpsOptions>(
            configuration.GetSection(AzureDevOpsOptions.SectionName));
        _ = services.Configure<TeamDefinitionOptions>(
            configuration.GetSection(TeamDefinitionOptions.SectionName));

        _ = services.AddSingleton(sp =>
        {
            AzureDevOpsOptions options = sp.GetRequiredService<IOptions<AzureDevOpsOptions>>().Value;

            var credentials = new VssBasicCredential(string.Empty, options.Pat);
            var connection = new VssConnection(new Uri(options.OrgUrl), credentials);

            return connection.GetClient<WorkItemTrackingHttpClient>();
        });

        _ = services.AddSingleton<IAzureDevOpsService, AzureDevOpsService>();

        return services;
    }
}
