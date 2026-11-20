namespace Smart.ComponentModel.DataAnnotations.AspNetCore;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

internal sealed class DefaultMessageMvcOptionsSetup : IPostConfigureOptions<MvcOptions>
{
    private readonly DefaultMessageLocalizer localizer;

    public DefaultMessageMvcOptionsSetup(DefaultMessageLocalizer localizer)
    {
        this.localizer = localizer;
    }

    public void PostConfigure(string? name, MvcOptions options)
    {
        options.ModelValidatorProviders.Add(new LocalizedModelValidatorProvider(localizer));

        var provider = options.ModelBindingMessageProvider;
        var missingBindRequiredValue = provider.MissingBindRequiredValueAccessor;
        provider.SetMissingBindRequiredValueAccessor(x => localizer.FormatModelBindingMessage("MissingBindRequiredValue", x) ?? missingBindRequiredValue(x));
        var missingKeyOrValue = provider.MissingKeyOrValueAccessor;
        provider.SetMissingKeyOrValueAccessor(() => localizer.FormatModelBindingMessage("MissingKeyOrValue") ?? missingKeyOrValue());
        var missingRequestBodyRequiredValue = provider.MissingRequestBodyRequiredValueAccessor;
        provider.SetMissingRequestBodyRequiredValueAccessor(() => localizer.FormatModelBindingMessage("MissingRequestBodyRequiredValue") ?? missingRequestBodyRequiredValue());
        var valueMustNotBeNull = provider.ValueMustNotBeNullAccessor;
        provider.SetValueMustNotBeNullAccessor(x => localizer.FormatModelBindingMessage("ValueMustNotBeNull", x) ?? valueMustNotBeNull(x));
        var attemptedValueIsInvalid = provider.AttemptedValueIsInvalidAccessor;
        provider.SetAttemptedValueIsInvalidAccessor((x, y) => localizer.FormatModelBindingMessage("AttemptedValueIsInvalid", x, y) ?? attemptedValueIsInvalid(x, y));
        var nonPropertyAttemptedValueIsInvalid = provider.NonPropertyAttemptedValueIsInvalidAccessor;
        provider.SetNonPropertyAttemptedValueIsInvalidAccessor(x => localizer.FormatModelBindingMessage("NonPropertyAttemptedValueIsInvalid", x) ?? nonPropertyAttemptedValueIsInvalid(x));
        var unknownValueIsInvalid = provider.UnknownValueIsInvalidAccessor;
        provider.SetUnknownValueIsInvalidAccessor(x => localizer.FormatModelBindingMessage("UnknownValueIsInvalid", x) ?? unknownValueIsInvalid(x));
        var nonPropertyUnknownValueIsInvalid = provider.NonPropertyUnknownValueIsInvalidAccessor;
        provider.SetNonPropertyUnknownValueIsInvalidAccessor(() => localizer.FormatModelBindingMessage("NonPropertyUnknownValueIsInvalid") ?? nonPropertyUnknownValueIsInvalid());
        var valueIsInvalid = provider.ValueIsInvalidAccessor;
        provider.SetValueIsInvalidAccessor(x => localizer.FormatModelBindingMessage("ValueIsInvalid", x) ?? valueIsInvalid(x));
        var valueMustBeANumber = provider.ValueMustBeANumberAccessor;
        provider.SetValueMustBeANumberAccessor(x => localizer.FormatModelBindingMessage("ValueMustBeANumber", x) ?? valueMustBeANumber(x));
        var nonPropertyValueMustBeANumber = provider.NonPropertyValueMustBeANumberAccessor;
        provider.SetNonPropertyValueMustBeANumberAccessor(() => localizer.FormatModelBindingMessage("NonPropertyValueMustBeANumber") ?? nonPropertyValueMustBeANumber());
    }
}
