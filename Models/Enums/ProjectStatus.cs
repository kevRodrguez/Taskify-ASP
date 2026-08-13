namespace Taskify.Models.Enums;

/// <summary>
/// Ciclo de vida del proyecto. No hay soft delete ni columna Archived:
/// el archivado es <see cref="Archived"/> (valor 2 en la columna Status).
/// </summary>
public enum ProjectStatus
{
    Active = 0,
    Completed = 1,
    Archived = 2
}
