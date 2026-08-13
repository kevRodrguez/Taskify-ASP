namespace Taskify.Models.Entities;

public class Profile
{
    public Guid ProfileId { get; set; }

    public Guid UserId { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public ICollection<Team> TeamsCreated { get; set; } = [];

    public ICollection<TeamMember> TeamMemberships { get; set; } = [];

    public ICollection<TaskItem> AssignedTasks { get; set; } = [];

    public ICollection<Notification> Notifications { get; set; } = [];
}
