using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Taskify.Models.Entities;

namespace Taskify.Data.Configurations;

public class TaskItemConfiguration : IEntityTypeConfiguration<TaskItem>
{
    public void Configure(EntityTypeBuilder<TaskItem> builder)
    {
        builder.HasKey(t => t.TaskItemId);

        builder.Property(t => t.TaskItemId)
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(t => t.Title)
            .HasMaxLength(200);

        builder.Property(t => t.Description)
            .HasMaxLength(2000);

        builder.HasQueryFilter(t => t.DeletedAt == null);

        builder.HasOne(t => t.Project)
            .WithMany(p => p.Tasks)
            .HasForeignKey(t => t.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(t => t.AssignedTo)
            .WithMany(p => p.AssignedTasks)
            .HasForeignKey(t => t.AssignedToProfileId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
