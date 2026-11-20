namespace Smart.ComponentModel.DataAnnotations.AspNetCore;

using Microsoft.AspNetCore.Mvc.DataAnnotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
#if NET11_0_OR_GREATER
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
#endif

public static class MvcBuilderExtensions
{
    public static IMvcBuilder AddValidationAttributeAdapters(this IMvcBuilder builder)
    {
        builder.Services.Replace(ServiceDescriptor.Singleton<IValidationAttributeAdapterProvider>(static provider => new AttributeAdapterProvider(provider.GetService<DefaultMessageLocalizer>())));
        return builder;
    }

#if NET11_0_OR_GREATER
    public static IMvcBuilder AddDefaultMessageLocalization(this IMvcBuilder builder)
    {
        builder.Services.TryAddSingleton(static provider => new DefaultMessageLocalizer(
            provider.GetRequiredService<IOptions<MvcDataAnnotationsLocalizationOptions>>(),
            provider.GetService<IStringLocalizerFactory>()));
        builder.Services.Replace(ServiceDescriptor.Singleton<IValidationAttributeAdapterProvider>(static provider => new AttributeAdapterProvider(provider.GetService<DefaultMessageLocalizer>())));
        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IPostConfigureOptions<MvcOptions>, DefaultMessageMvcOptionsSetup>(static provider => new DefaultMessageMvcOptionsSetup(provider.GetRequiredService<DefaultMessageLocalizer>())));
        return builder;
    }
#endif
}
