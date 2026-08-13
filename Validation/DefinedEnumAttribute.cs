using System.ComponentModel.DataAnnotations;

namespace Taskify.Validation;

/// <summary>
/// Rechaza enteros que no correspondan a un miembro definido del enum (p. ej. Role = 45).
/// Usar en ViewModels que reciben enums desde formularios o APIs.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class DefinedEnumAttribute : ValidationAttribute
{
    private readonly Type _enumType;

    public DefinedEnumAttribute(Type enumType)
    {
        if (!enumType.IsEnum)
        {
            throw new ArgumentException("El tipo debe ser un enum.", nameof(enumType));
        }

        _enumType = enumType;
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is null)
        {
            return ValidationResult.Success;
        }

        if (!EnumValidator.IsDefined(_enumType, value))
        {
            var message = ErrorMessage
                ?? $"El valor no es válido para {validationContext.DisplayName ?? _enumType.Name}.";

            return new ValidationResult(message, [validationContext.MemberName!]);
        }

        return ValidationResult.Success;
    }
}
