using Taskify.Models.Enums;

namespace Taskify.Models.Entities;

public class TeamMember
{
    public Guid TeamId { get; set; }

    public Guid ProfileId { get; set; }

    public TeamMemberRole Role { get; set; }

    public DateTimeOffset JoinedAt { get; set; }

    public Team Team { get; set; } = null!;

    public Profile Profile { get; set; } = null!;
}
