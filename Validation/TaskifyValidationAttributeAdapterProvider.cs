using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.DataAnnotations;
using Microsoft.Extensions.Localization;

namespace Taskify.Validation;

public sealed class TaskifyValidationAttributeAdapterProvider : IValidationAttributeAdapterProvider
{
    private readonly ValidationAttributeAdapterProvider _inner = new();

    public IAttributeAdapter? GetAttributeAdapter(ValidationAttribute attribute, IStringLocalizer? stringLocalizer)
    {
        if (attribute is DateNotBeforeAttribute dateNotBefore)
        {
            return new DateNotBeforeAttributeAdapter(dateNotBefore, stringLocalizer);
        }

        return _inner.GetAttributeAdapter(attribute, stringLocalizer);
    }
}
