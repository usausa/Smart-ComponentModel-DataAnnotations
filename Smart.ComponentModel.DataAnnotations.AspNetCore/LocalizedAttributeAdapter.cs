namespace Smart.ComponentModel.DataAnnotations.AspNetCore;

using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Mvc.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

internal sealed class LocalizedAttributeAdapter : IAttributeAdapter
{
    private const string MessagePrefix = "data-val-";

    private readonly IAttributeAdapter adapter;

    private readonly ValidationAttribute attribute;

    private readonly DefaultMessageLocalizer localizer;

    public LocalizedAttributeAdapter(IAttributeAdapter adapter, ValidationAttribute attribute, DefaultMessageLocalizer localizer)
    {
        this.adapter = adapter;
        this.attribute = attribute;
        this.localizer = localizer;
    }

    public void AddValidation(ClientModelValidationContext context)
    {
        var keys = new HashSet<string>(context.Attributes.Keys, StringComparer.Ordinal);
        adapter.AddValidation(context);
        if (ValidationMessageResolver.FormatMessage(attribute, context, localizer) is not { } message)
        {
            return;
        }

        foreach (var key in context.Attributes.Keys.Where(x => !keys.Contains(x) && IsMessageKey(x)).ToList())
        {
            context.Attributes[key] = message;
        }
    }

    public string GetErrorMessage(ModelValidationContextBase validationContext) =>
        ValidationMessageResolver.FormatMessage(attribute, validationContext, localizer) ?? adapter.GetErrorMessage(validationContext);

    private static bool IsMessageKey(string key) =>
        key.StartsWith(MessagePrefix, StringComparison.Ordinal) && (key.AsSpan(MessagePrefix.Length).IndexOf('-') < 0);
}
