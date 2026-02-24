using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace CabaVS.AzureDevOpsHelper.Presentation;

public static class DependencyInjection
{
    public static IServiceCollection AddPresentation(this IServiceCollection services, bool isDevelopment = false)
    {
        _ = services.AddFastEndpoints();

        if (isDevelopment)
        {
            _ = services.SwaggerDocument(opts => opts.AutoTagPathSegmentIndex = 0);
        }

        return services;
    }

    public static IApplicationBuilder UsePresentation(this IApplicationBuilder application, bool isDevelopment = false)
    {
        _ = application.UseFastEndpoints();
        
        if (isDevelopment)
        {
            _ = application.UseSwaggerGen();
        }

        return application;
    }
}
