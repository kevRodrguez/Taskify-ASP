using Taskify.Models.Enums;

namespace Taskify.ViewModels.Teams;

public class TeamMemberListItemViewModel
{
    public Guid ProfileId { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public TeamMemberRole Role { get; set; }

    public DateTimeOffset JoinedAt { get; set; }
}
