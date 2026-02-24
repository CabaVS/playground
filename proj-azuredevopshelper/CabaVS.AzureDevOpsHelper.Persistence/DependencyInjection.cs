using CabaVS.AzureDevOpsHelper.Application.Contracts.Persistence;
using CabaVS.AzureDevOpsHelper.Application.Contracts.Persistence.Repositories;
using CabaVS.AzureDevOpsHelper.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CabaVS.AzureDevOpsHelper.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("postgresdb");

        _ = services.AddDbContext<ApplicationDbContext>(cfg => cfg.UseNpgsql(connectionString), ServiceLifetime.Scoped);

        _ = services.AddScoped<IUnitOfWork, UnitOfWork>();

        _ = services.AddScoped<IUserReadRepository, UserReadRepository>();

        return services;
    }
}
