using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Taskify.Models.Entities;

namespace Taskify.Data.Configurations;

public class AuthUserConfiguration : IEntityTypeConfiguration<AuthUser>
{
    public void Configure(EntityTypeBuilder<AuthUser> builder)
    {
        builder.ToTable("users", "auth", t => t.ExcludeFromMigrations());

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id)
            .HasColumnName("id");
    }
}
