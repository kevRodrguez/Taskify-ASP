using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Taskify.Models.Entities;

namespace Taskify.Data.Configurations;

public class ProfileConfiguration : IEntityTypeConfiguration<Profile>
{
    public void Configure(EntityTypeBuilder<Profile> builder)
    {
        builder.HasKey(p => p.ProfileId);

        builder.Property(p => p.ProfileId)
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(p => p.FullName)
            .HasMaxLength(200);

        builder.Property(p => p.Email)
            .HasMaxLength(256);

        builder.HasOne(p => p.AuthUser)
            .WithOne(u => u.Profile)
            .HasForeignKey<Profile>(p => p.UserId)
            .HasPrincipalKey<AuthUser>(u => u.Id)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(p => p.UserId).IsUnique();
        builder.HasIndex(p => p.Email).IsUnique();
    }
}
