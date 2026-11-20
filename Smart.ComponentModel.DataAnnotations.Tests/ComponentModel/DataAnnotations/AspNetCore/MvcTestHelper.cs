namespace Smart.ComponentModel.DataAnnotations.AspNetCore;

using System.Reflection;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;

internal static class MvcTestHelper
{
    public static ServiceProvider CreateServices() =>
        CreateServices(null);

    public static ServiceProvider CreateServices(Dictionary<string, string>? messages)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        var builder = services.AddControllers();
        if (messages is not null)
        {
            var localizer = new TestStringLocalizer(messages);
            builder.AddDataAnnotationsLocalization(options => options.DataAnnotationLocalizerProvider = (_, _) => localizer);
            builder.AddDefaultMessageLocalization();
        }

        builder.AddValidationAttributeAdapters();
        return services.BuildServiceProvider();
    }

    public static Dictionary<string, string> AddValidation(IServiceProvider provider, Type type, string property)
    {
        var metadataProvider = provider.GetRequiredService<IModelMetadataProvider>();
        var adapterProvider = provider.GetRequiredService<IValidationAttributeAdapterProvider>();
        var metadata = metadataProvider.GetMetadataForProperty(type, property);
        var attributes = new Dictionary<string, string>();
        var context = new ClientModelValidationContext(new ActionContext(), metadata, metadataProvider, attributes);
        var stringLocalizer = CreateStringLocalizer(provider, type);
        foreach (var attribute in metadata.ValidatorMetadata.OfType<ValidationAttribute>())
        {
            adapterProvider.GetAttributeAdapter(attribute, stringLocalizer)?.AddValidation(context);
        }

        return attributes;
    }

    public static ModelStateDictionary Validate(IServiceProvider provider, object model)
    {
        var actionContext = CreateActionContext(provider);
        provider.GetRequiredService<IObjectModelValidator>().Validate(actionContext, null, string.Empty, model);
        return actionContext.ModelState;
    }

    public static ModelStateDictionary ValidateParameter(IServiceProvider provider, object model, ParameterInfo parameter)
    {
        var metadata = ((ModelMetadataProvider)provider.GetRequiredService<IModelMetadataProvider>()).GetMetadataForParameter(parameter);
        var actionContext = CreateActionContext(provider);
        ((ObjectModelValidator)provider.GetRequiredService<IObjectModelValidator>()).Validate(actionContext, null, string.Empty, model, metadata);
        return actionContext.ModelState;
    }

    public static string? GetError(ModelStateDictionary modelState, string key) =>
        modelState.TryGetValue(key, out var entry) && (entry.Errors.Count > 0) ? entry.Errors[0].ErrorMessage : null;

    private static ActionContext CreateActionContext(IServiceProvider provider) =>
        new(new DefaultHttpContext { RequestServices = provider }, new RouteData(), new ActionDescriptor());

    private static IStringLocalizer? CreateStringLocalizer(IServiceProvider provider, Type type)
    {
        var options = provider.GetRequiredService<IOptions<MvcDataAnnotationsLocalizationOptions>>().Value;
        var factory = provider.GetService<IStringLocalizerFactory>();
        return (factory is not null) && (options.DataAnnotationLocalizerProvider is { } localizerProvider) ? localizerProvider(type, factory) : null;
    }
}
