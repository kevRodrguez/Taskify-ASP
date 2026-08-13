using Taskify.Models.Enums;

namespace Taskify.ViewModels.Teams;

public class TeamListItemViewModel
{
    public Guid TeamId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public int MemberCount { get; set; }

    public int ProjectCount { get; set; }

    public TeamMemberRole Role { get; set; }
}
