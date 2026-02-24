using CabaVS.AzureDevOpsHelper.Application.Behaviors;
using CabaVS.AzureDevOpsHelper.Application.Contracts.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace CabaVS.AzureDevOpsHelper.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        _ = services.AddMediatR(cfg =>
        {
            _ = cfg.RegisterServicesFromAssembly(AssemblyMarker.Application);

            _ = cfg.AddOpenBehaviors(
                [
                    typeof(LoggingBehavior<,>),
                    typeof(EnsureLocalUserBehavior<,>)
                ],
                ServiceLifetime.Scoped);

            cfg.Lifetime = ServiceLifetime.Scoped;
        });

        // WIP: Implement IUserAccessor and replace DummyUserAccessor with the actual implementation.
        _ = services.AddScoped<IUserAccessor, DummyUserAccessor>();
    
        return services;
    }
}
