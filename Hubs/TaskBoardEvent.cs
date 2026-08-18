using Taskify.Models.Enums;

namespace Taskify.Hubs;

public sealed class TaskBoardEvent
{
    public Guid TaskItemId { get; set; }

    public Guid ProjectId { get; set; }

    public TaskItemStatus Status { get; set; }

    public int SortOrder { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? AssignedToName { get; set; }

    public DateOnly? DueDate { get; set; }

    public Guid? ClientRequestId { get; set; }

    public bool Deleted { get; set; }
}
