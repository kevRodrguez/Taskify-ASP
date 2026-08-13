using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Taskify.Models.Entities;

namespace Taskify.Data.Configurations;

public class TeamConfiguration : IEntityTypeConfiguration<Team>
{
    public void Configure(EntityTypeBuilder<Team> builder)
    {
        builder.HasKey(t => t.TeamId);

        builder.Property(t => t.TeamId)
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(t => t.Name)
            .HasMaxLength(120);

        builder.Property(t => t.Description)
            .HasMaxLength(500);

        // Soft delete: consultas normales omiten equipos con DeletedAt distinto de null.
        builder.HasQueryFilter(t => t.DeletedAt == null);

        builder.HasOne(t => t.Creator)
            .WithMany(p => p.TeamsCreated)
            .HasForeignKey(t => t.CreatedBy)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
