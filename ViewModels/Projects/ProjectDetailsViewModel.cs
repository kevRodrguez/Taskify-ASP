using Taskify.Models.Enums;

namespace Taskify.ViewModels.Projects;

public class ProjectDetailsViewModel
{
    public Guid ProjectId { get; set; }

    public Guid TeamId { get; set; }

    public string TeamName { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly DueDate { get; set; }

    public ProjectStatus Status { get; set; }

    public bool CanManage { get; set; }
}
