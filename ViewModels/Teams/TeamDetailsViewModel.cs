using Taskify.Models.Enums;

namespace Taskify.ViewModels.Teams;

public class TeamDetailsViewModel
{
    public Guid TeamId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public TeamMemberRole CurrentUserRole { get; set; }

    public bool CanManage { get; set; }

    public bool IsOwner { get; set; }

    public IReadOnlyList<TeamMemberListItemViewModel> Members { get; set; } = [];

    public AddTeamMemberViewModel AddMember { get; set; } = new();
}
