namespace Smart.ComponentModel.DataAnnotations.AspNetCore.Adapters;

using System.ComponentModel.DataAnnotations;
using System.Globalization;

using Microsoft.AspNetCore.Mvc.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.Extensions.Localization;

internal abstract class AttributeAdapter<TAttribute> : AttributeAdapterBase<TAttribute>
    where TAttribute : ValidationAttribute
{
    private readonly IStringLocalizer? stringLocalizer;

    private readonly DefaultMessageLocalizer? localizer;

    protected AttributeAdapter(TAttribute attribute, IStringLocalizer? stringLocalizer, DefaultMessageLocalizer? localizer)
        : base(attribute, stringLocalizer)
    {
        this.stringLocalizer = stringLocalizer;
        this.localizer = localizer;
    }

    public override string GetErrorMessage(ModelValidationContextBase validationContext)
    {
        var template = FindTemplate(validationContext);
        if (Attribute is IMemberMessageFormatter formatter)
        {
            return ValidationMessageResolver.FormatMemberMessage(formatter, template, validationContext);
        }

        var displayName = validationContext.ModelMetadata.GetDisplayName();
        return template is null
            ? Attribute.FormatErrorMessage(displayName)
            : String.Format(CultureInfo.CurrentCulture, template, [displayName, .. GetMessageArguments()]);
    }

    protected virtual object[] GetMessageArguments() => [];

    protected static string ToBoolean(bool value) => value ? "true" : "false";

    private string? FindTemplate(ModelValidationContextBase validationContext)
    {
        if (localizer?.FindTemplate(Attribute, validationContext.ModelMetadata) is { } template)
        {
            return template;
        }

        if ((stringLocalizer is null) ||
            String.IsNullOrEmpty(Attribute.ErrorMessage) ||
            !String.IsNullOrEmpty(Attribute.ErrorMessageResourceName) ||
            (Attribute.ErrorMessageResourceType is not null))
        {
            return null;
        }

        return stringLocalizer[Attribute.ErrorMessage].Value;
    }
}
