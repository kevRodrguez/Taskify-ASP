namespace Taskify.Models.Enums;

/// <summary>Valores válidos en BD: 0, 1, 2. Ver <see cref="Taskify.Validation.DefinedEnumAttribute"/>.</summary>
public enum TaskItemStatus
{
    Todo = 0,
    InProgress = 1,
    Done = 2
}
