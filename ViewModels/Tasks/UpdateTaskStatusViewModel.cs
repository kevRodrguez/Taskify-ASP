using System.ComponentModel.DataAnnotations;
using Taskify.Models.Enums;
using Taskify.Validation;

namespace Taskify.ViewModels.Tasks;

public class UpdateTaskStatusViewModel
{
    [Required]
    public Guid TaskItemId { get; set; }

    [Required]
    public Guid ProjectId { get; set; }

    [Required]
    [DefinedEnum(typeof(TaskItemStatus), ErrorMessage = "El estado no es válido.")]
    public TaskItemStatus Status { get; set; }

    public int SortOrder { get; set; }

    public Guid? ClientRequestId { get; set; }
}
