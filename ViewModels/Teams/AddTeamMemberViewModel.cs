using System.ComponentModel.DataAnnotations;
using Taskify.Models.Enums;
using Taskify.Validation;

namespace Taskify.ViewModels.Teams;

public class AddTeamMemberViewModel
{
    public Guid TeamId { get; set; }

    [Required(ErrorMessage = "El correo es obligatorio.")]
    [EmailAddress(ErrorMessage = "El correo electrónico no es válido.")]
    [Display(Name = "Correo del miembro")]
    public string Email { get; set; } = string.Empty;

    [Required]
    [DefinedEnum(typeof(TeamMemberRole), ErrorMessage = "El rol no es válido.")]
    [Display(Name = "Rol")]
    public TeamMemberRole Role { get; set; } = TeamMemberRole.Member;
}
