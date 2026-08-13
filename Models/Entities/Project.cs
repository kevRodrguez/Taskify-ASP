using Taskify.Models.Enums;

namespace Taskify.Models.Entities;

public class Project
{
    public Guid ProjectId { get; set; }

    public Guid TeamId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly DueDate { get; set; }

    /// <summary>
    /// Ciclo de vida del proyecto. Archivar = <see cref="ProjectStatus.Archived"/> (no hay DeletedAt).
    /// </summary>
    public ProjectStatus Status { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public Team Team { get; set; } = null!;

    public ICollection<TaskItem> Tasks { get; set; } = [];
}
