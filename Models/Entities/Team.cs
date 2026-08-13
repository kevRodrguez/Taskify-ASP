namespace Taskify.Models.Entities;

public class Team
{
    public Guid TeamId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public Guid CreatedBy { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// Soft delete: null = equipo activo; timestamp = borrado (no se elimina la fila).
    /// </summary>
    public DateTimeOffset? DeletedAt { get; set; }

    public Profile Creator { get; set; } = null!;

    public ICollection<TeamMember> Members { get; set; } = [];

    public ICollection<Project> Projects { get; set; } = [];
}
