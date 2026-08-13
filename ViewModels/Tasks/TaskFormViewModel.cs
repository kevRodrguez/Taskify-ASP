using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Taskify.Models.Enums;
using Taskify.Validation;

namespace Taskify.ViewModels.Tasks;

public class TaskFormViewModel
{
    public Guid? TaskItemId { get; set; }

    [Required]
    public Guid ProjectId { get; set; }

    public string ProjectName { get; set; } = string.Empty;

    [Required(ErrorMessage = "El título es obligatorio.")]
    [StringLength(200, ErrorMessage = "El título no puede superar los 200 caracteres.")]
    [Display(Name = "Título")]
    public string Title { get; set; } = string.Empty;

    [StringLength(2000, ErrorMessage = "La descripción no puede superar los 2000 caracteres.")]
    [Display(Name = "Descripción")]
    [DataType(DataType.MultilineText)]
    public string? Description { get; set; }

    [Required]
    [DefinedEnum(typeof(TaskItemStatus), ErrorMessage = "El estado no es válido.")]
    [Display(Name = "Estado")]
    public TaskItemStatus Status { get; set; } = TaskItemStatus.Todo;

    [DataType(DataType.Date)]
    [Display(Name = "Fecha de vencimiento")]
    public DateOnly? DueDate { get; set; }

    [Display(Name = "Asignado a")]
    public Guid? AssignedToProfileId { get; set; }

    public IEnumerable<SelectListItem> Assignees { get; set; } = [];
}
