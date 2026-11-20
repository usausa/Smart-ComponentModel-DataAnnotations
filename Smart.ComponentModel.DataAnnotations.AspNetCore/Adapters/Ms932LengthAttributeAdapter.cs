namespace Smart.ComponentModel.DataAnnotations.AspNetCore.Adapters;

using System.ComponentModel.DataAnnotations;
using System.Globalization;

using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.Extensions.Localization;

internal sealed class Ms932LengthAttributeAdapter<TAttribute> : AttributeAdapter<TAttribute>
    where TAttribute : ValidationAttribute
{
    private readonly int minimum;

    private readonly int maximum;

    private readonly object[] arguments;

    public Ms932LengthAttributeAdapter(TAttribute attribute, int minimum, int maximum, object[] arguments, IStringLocalizer? stringLocalizer, DefaultMessageLocalizer? localizer)
        : base(attribute, stringLocalizer, localizer)
    {
        this.minimum = minimum;
        this.maximum = maximum;
        this.arguments = arguments;
    }

    public override void AddValidation(ClientModelValidationContext context)
    {
        MergeAttribute(context.Attributes, "data-val", "true");
        MergeAttribute(context.Attributes, "data-val-ms932length", GetErrorMessage(context));
        MergeAttribute(context.Attributes, "data-val-ms932length-min", minimum.ToString(CultureInfo.InvariantCulture));
        MergeAttribute(context.Attributes, "data-val-ms932length-max", maximum.ToString(CultureInfo.InvariantCulture));
    }

    protected override object[] GetMessageArguments() => arguments;
}
