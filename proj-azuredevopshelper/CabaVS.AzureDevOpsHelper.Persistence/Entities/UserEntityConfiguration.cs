using CabaVS.AzureDevOpsHelper.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CabaVS.AzureDevOpsHelper.Persistence.Entities;

internal sealed class UserEntityConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users", "azdh");

        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id)
            .HasColumnName("id")
            .HasColumnType("uuid")
            .IsRequired();

        builder.Property(u => u.ExternalId)
            .HasColumnName("external_id")
            .HasColumnType("uuid")
            .IsRequired();
        builder.HasIndex(u => u.ExternalId)
            .IsUnique();

    }
}
