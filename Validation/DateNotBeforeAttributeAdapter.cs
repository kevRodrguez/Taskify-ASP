using Microsoft.AspNetCore.Mvc.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.Extensions.Localization;

namespace Taskify.Validation;

public sealed class DateNotBeforeAttributeAdapter : AttributeAdapterBase<DateNotBeforeAttribute>
{
    public DateNotBeforeAttributeAdapter(DateNotBeforeAttribute attribute, IStringLocalizer? stringLocalizer)
        : base(attribute, stringLocalizer)
    {
    }

    public override void AddValidation(ClientModelValidationContext context)
    {
        MergeAttribute(context.Attributes, "data-val", "true");
        MergeAttribute(context.Attributes, "data-val-datenotbefore", GetErrorMessage(context));

        var other = context.Attributes.TryGetValue("name", out var currentName) && currentName is not null
            ? BuildOtherFieldName(currentName, Attribute.OtherPropertyName)
            : Attribute.OtherPropertyName;

        MergeAttribute(context.Attributes, "data-val-datenotbefore-other", other);
    }

    public override string GetErrorMessage(ModelValidationContextBase validationContext)
    {
        var otherDisplay = Attribute.OtherPropertyName;
        var otherProperty = validationContext.ModelMetadata.ContainerType?.GetProperty(Attribute.OtherPropertyName);
        if (otherProperty is not null)
        {
            var display = otherProperty
                .GetCustomAttributes(typeof(System.ComponentModel.DataAnnotations.DisplayAttribute), true)
                .OfType<System.ComponentModel.DataAnnotations.DisplayAttribute>()
                .FirstOrDefault()
                ?.GetName();

            if (!string.IsNullOrEmpty(display))
            {
                otherDisplay = display;
            }
        }

        return GetErrorMessage(validationContext.ModelMetadata, otherDisplay);
    }

    private static string BuildOtherFieldName(string currentName, string otherPropertyName)
    {
        var lastDot = currentName.LastIndexOf('.');
        if (lastDot < 0)
        {
            return otherPropertyName;
        }

        return currentName[..(lastDot + 1)] + otherPropertyName;
    }
}
