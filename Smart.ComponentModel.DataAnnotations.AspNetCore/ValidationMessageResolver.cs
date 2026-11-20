namespace Smart.ComponentModel.DataAnnotations.AspNetCore;

using System.ComponentModel.DataAnnotations;
using System.Globalization;

using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

internal static class ValidationMessageResolver
{
    public static string? FormatMessage(ValidationAttribute attribute, ModelValidationContextBase context, DefaultMessageLocalizer localizer)
    {
        var template = localizer.FindTemplate(attribute, context.ModelMetadata);
        if (attribute is IMemberMessageFormatter formatter)
        {
            return FormatMemberMessage(formatter, template, context);
        }

        if (template is null)
        {
            return null;
        }

        var displayName = context.ModelMetadata.GetDisplayName();
        if (attribute is CompareAttribute compare)
        {
            return String.Format(CultureInfo.CurrentCulture, template, displayName, ResolveDisplayName(context, context.ModelMetadata.ContainerType, compare.OtherProperty));
        }

#if NET11_0_OR_GREATER
        return attribute.FormatMessage(template, displayName);
#else
        return null;
#endif
    }

    public static string FormatMemberMessage(IMemberMessageFormatter formatter, string? template, ModelValidationContextBase context)
    {
        var metadata = context.ModelMetadata;
        var containerType = formatter.AppliesToType ? metadata.UnderlyingOrModelType : metadata.ContainerType;
        return formatter.FormatMessage(template, metadata.GetDisplayName(), [.. formatter.MessageMembers.Select(x => ResolveDisplayName(context, containerType, x))]);
    }

    private static string ResolveDisplayName(ModelValidationContextBase context, Type? containerType, string member) =>
        containerType is null
            ? member
            : context.MetadataProvider.GetMetadataForType(containerType).Properties[member]?.GetDisplayName() ?? member;
}
