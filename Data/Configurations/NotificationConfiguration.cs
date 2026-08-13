using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Taskify.Models.Entities;

namespace Taskify.Data.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.HasKey(n => n.NotificationId);

        builder.Property(n => n.NotificationId)
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(n => n.Message)
            .HasMaxLength(500);

        builder.Property(n => n.IsRead)
            .HasDefaultValue(false);

        builder.HasOne(n => n.Profile)
            .WithMany(p => p.Notifications)
            .HasForeignKey(n => n.ProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(n => n.TaskItem)
            .WithMany(t => t.Notifications)
            .HasForeignKey(n => n.TaskItemId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
