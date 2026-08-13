using Taskify.Models.Enums;

namespace Taskify.ViewModels.Tasks;

public class TaskCardViewModel
{
    public Guid TaskItemId { get; set; }

    public Guid ProjectId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public TaskItemStatus Status { get; set; }

    public int SortOrder { get; set; }

    public DateOnly? DueDate { get; set; }

    public Guid? AssignedToProfileId { get; set; }

    public string? AssignedToName { get; set; }
}
