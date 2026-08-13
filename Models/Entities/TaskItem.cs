using Taskify.Models.Enums;

namespace Taskify.Models.Entities;

public class TaskItem
{
    public Guid TaskItemId { get; set; }

    public Guid ProjectId { get; set; }

    public Guid? AssignedToProfileId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public TaskItemStatus Status { get; set; }

    public int SortOrder { get; set; }

    public DateOnly? DueDate { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public Project Project { get; set; } = null!;

    public Profile? AssignedTo { get; set; }

    public ICollection<Notification> Notifications { get; set; } = [];
}
