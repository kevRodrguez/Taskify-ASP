using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Taskify.Models.Entities;

namespace Taskify.Data.Configurations;

public class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.HasKey(p => p.ProjectId);

        builder.Property(p => p.ProjectId)
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(p => p.Name)
            .HasMaxLength(200);

        builder.Property(p => p.Description)
            .HasMaxLength(1000);

        // Projects no usan DeletedAt; archivar/completar se modela con ProjectStatus (Active/Completed/Archived).

        builder.ToTable(t => t.HasCheckConstraint("CK_Projects_DueDate", "\"DueDate\" >= \"StartDate\""));

        builder.HasOne(p => p.Team)
            .WithMany(t => t.Projects)
            .HasForeignKey(p => p.TeamId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
