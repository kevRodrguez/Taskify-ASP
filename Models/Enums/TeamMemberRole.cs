namespace Taskify.Models.Enums;

/// <summary>Valores válidos en BD: 0, 1, 2. Ver <see cref="Taskify.Validation.DefinedEnumAttribute"/>.</summary>
public enum TeamMemberRole
{
    Owner = 0,
    Admin = 1,
    Member = 2
}
