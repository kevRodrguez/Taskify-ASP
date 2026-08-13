using System.ComponentModel.DataAnnotations;

namespace Taskify.ViewModels.Teams;

public class TeamFormViewModel
{
    public Guid? TeamId { get; set; }

    [Required(ErrorMessage = "El nombre del equipo es obligatorio.")]
    [StringLength(120, ErrorMessage = "El nombre no puede superar los 120 caracteres.")]
    [Display(Name = "Nombre")]
    public string Name { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "La descripción no puede superar los 500 caracteres.")]
    [Display(Name = "Descripción")]
    [DataType(DataType.MultilineText)]
    public string? Description { get; set; }
}
