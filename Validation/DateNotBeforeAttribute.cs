using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace Taskify.Validation;

/// <summary>
/// Exige que la fecha de esta propiedad no sea anterior a la de <paramref name="otherPropertyName"/>.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class DateNotBeforeAttribute : ValidationAttribute
{
    public DateNotBeforeAttribute(string otherPropertyName)
    {
        OtherPropertyName = otherPropertyName;
        ErrorMessage = "La fecha no puede ser anterior a {0}.";
    }

    public string OtherPropertyName { get; }

    public override bool RequiresValidationContext => true;

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is null)
        {
            return ValidationResult.Success;
        }

        var otherProperty = validationContext.ObjectType.GetProperty(OtherPropertyName);
        if (otherProperty is null)
        {
            throw new InvalidOperationException($"No existe la propiedad '{OtherPropertyName}'.");
        }

        var otherValue = otherProperty.GetValue(validationContext.ObjectInstance);
        if (otherValue is null)
        {
            return ValidationResult.Success;
        }

        var current = ToDate(value);
        var other = ToDate(otherValue);
        if (current is null || other is null)
        {
            return ValidationResult.Success;
        }

        if (current < other)
        {
            var otherDisplay = otherProperty
                .GetCustomAttributes(typeof(DisplayAttribute), true)
                .OfType<DisplayAttribute>()
                .FirstOrDefault()
                ?.GetName() ?? OtherPropertyName;

            var message = string.Format(CultureInfo.CurrentCulture, ErrorMessageString, otherDisplay);
            return new ValidationResult(message, [validationContext.MemberName!]);
        }

        return ValidationResult.Success;
    }

    private static DateOnly? ToDate(object value)
    {
        return value switch
        {
            DateOnly dateOnly => dateOnly,
            DateTime dateTime => DateOnly.FromDateTime(dateTime),
            DateTimeOffset dateTimeOffset => DateOnly.FromDateTime(dateTimeOffset.DateTime),
            _ => null
        };
    }
}
