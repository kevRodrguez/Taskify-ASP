using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Taskify.Models.Entities;

namespace Taskify.Data.Configurations;

public class TeamMemberConfiguration : IEntityTypeConfiguration<TeamMember>
{
    public void Configure(EntityTypeBuilder<TeamMember> builder)
    {
        builder.HasKey(tm => new { tm.TeamId, tm.ProfileId });

        builder.HasOne(tm => tm.Team)
            .WithMany(t => t.Members)
            .HasForeignKey(tm => tm.TeamId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(tm => tm.Profile)
            .WithMany(p => p.TeamMemberships)
            .HasForeignKey(tm => tm.ProfileId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
