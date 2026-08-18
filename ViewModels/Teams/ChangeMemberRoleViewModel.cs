using System.ComponentModel.DataAnnotations;
using Taskify.Models.Enums;
using Taskify.Validation;

namespace Taskify.ViewModels.Teams;

public class ChangeMemberRoleViewModel
{
    [Required]
    public Guid TeamId { get; set; }

    [Required]
    public Guid ProfileId { get; set; }

    [Required]
    [DefinedEnum(typeof(TeamMemberRole), ErrorMessage = "El rol no es válido.")]
    public TeamMemberRole Role { get; set; }
}
