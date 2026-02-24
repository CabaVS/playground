using CabaVS.AzureDevOpsHelper.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CabaVS.AzureDevOpsHelper.Persistence;

internal sealed class ApplicationDbContext : DbContext
{
    public DbSet<User> Users { get; set; }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        _ = modelBuilder.ApplyConfigurationsFromAssembly(AssemblyMarker.Persistence);
    }
}
