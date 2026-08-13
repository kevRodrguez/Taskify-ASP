using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Taskify.Validation;

namespace Taskify.ViewModels.Projects;

public class ProjectFormViewModel
{
    public Guid? ProjectId { get; set; }

    [Required(ErrorMessage = "El equipo es obligatorio.")]
    [Display(Name = "Equipo")]
    public Guid TeamId { get; set; }

    [Required(ErrorMessage = "El nombre del proyecto es obligatorio.")]
    [StringLength(200, ErrorMessage = "El nombre no puede superar los 200 caracteres.")]
    [Display(Name = "Nombre")]
    public string Name { get; set; } = string.Empty;

    [StringLength(1000, ErrorMessage = "La descripción no puede superar los 1000 caracteres.")]
    [Display(Name = "Descripción")]
    [DataType(DataType.MultilineText)]
    public string? Description { get; set; }

    [Required(ErrorMessage = "La fecha de inicio es obligatoria.")]
    [DataType(DataType.Date)]
    [Display(Name = "Fecha de inicio")]
    public DateOnly StartDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    [Required(ErrorMessage = "La fecha de vencimiento es obligatoria.")]
    [DataType(DataType.Date)]
    [Display(Name = "Fecha de vencimiento")]
    [DateNotBefore(nameof(StartDate), ErrorMessage = "La fecha de vencimiento no puede ser anterior a {0}.")]
    public DateOnly DueDate { get; set; } = DateOnly.FromDateTime(DateTime.Today.AddDays(14));

    public IEnumerable<SelectListItem> Teams { get; set; } = [];
}
