namespace Smart.ComponentModel.DataAnnotations.AspNetCore;

using System.ComponentModel.DataAnnotations;
using System.Globalization;

using Microsoft.AspNetCore.Mvc.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;

internal sealed class DefaultMessageLocalizer
{
    private readonly IOptions<MvcDataAnnotationsLocalizationOptions> options;

    private readonly IStringLocalizerFactory? factory;

    public DefaultMessageLocalizer(IOptions<MvcDataAnnotationsLocalizationOptions> options, IStringLocalizerFactory? factory)
    {
        this.options = options;
        this.factory = factory;
    }

    public string? FindTemplate(ValidationAttribute attribute, ModelMetadata metadata)
    {
        if (attribute.ErrorMessageResourceType is not null)
        {
            return null;
        }

        var type = metadata.ContainerType ?? metadata.ModelType;
        if (CreateLocalizer(type) is not { } localizer)
        {
            return null;
        }

        if (!String.IsNullOrEmpty(attribute.ErrorMessage) && (Find(localizer, attribute.ErrorMessage) is { } explicitMatch))
        {
            return explicitMatch;
        }

        var attributeName = attribute.GetType().Name;
        var typeName = GetKeySegment(type);
        if ((metadata.Name is { } memberName) && (Find(localizer, $"{typeName}_{memberName}_{attributeName}_Error") is { } memberMatch))
        {
            return memberMatch;
        }

        return Find(localizer, $"{typeName}_{attributeName}_Error") ?? Find(localizer, $"{attributeName}_Error");
    }

    public string? FormatModelBindingMessage(string name, params object?[] arguments)
    {
        if ((CreateLocalizer(typeof(ModelBindingMessageProvider)) is not { } localizer) || (Find(localizer, $"{name}_Error") is not { } template))
        {
            return null;
        }

        return String.Format(CultureInfo.CurrentCulture, template, arguments);
    }

    private IStringLocalizer? CreateLocalizer(Type type) =>
        (factory is not null) && (options.Value.DataAnnotationLocalizerProvider is { } provider) ? provider(type, factory) : null;

    private static string? Find(IStringLocalizer localizer, string key)
    {
        var value = localizer[key];
        return value.ResourceNotFound ? null : value.Value;
    }

    private static string GetKeySegment(Type type)
    {
        var name = (Nullable.GetUnderlyingType(type) ?? type).Name;
        var index = name.IndexOf('`', StringComparison.Ordinal);
        return index < 0 ? name : name[..index];
    }
}
