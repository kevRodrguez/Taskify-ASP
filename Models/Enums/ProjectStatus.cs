namespace Taskify.Models.Enums;

/// <summary>
/// Ciclo de vida del proyecto. No hay soft delete ni columna Archived:
/// el archivado es <see cref="Archived"/> (valor 2 en la columna Status).
/// Valores válidos en BD: 0, 1, 2. Ver <see cref="Taskify.Validation.DefinedEnumAttribute"/>.
/// </summary>
public enum ProjectStatus
{
    Active = 0,
    Completed = 1,
    Archived = 2
}
