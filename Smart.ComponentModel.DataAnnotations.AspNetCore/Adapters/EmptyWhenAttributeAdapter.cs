namespace Smart.ComponentModel.DataAnnotations.AspNetCore.Adapters;

using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.Extensions.Localization;

internal sealed class EmptyWhenAttributeAdapter : AttributeAdapter<EmptyWhenAttribute>
{
    public EmptyWhenAttributeAdapter(EmptyWhenAttribute attribute, IStringLocalizer? stringLocalizer, DefaultMessageLocalizer? localizer)
        : base(attribute, stringLocalizer, localizer)
    {
    }

    public override void AddValidation(ClientModelValidationContext context)
    {
        MergeAttribute(context.Attributes, "data-val", "true");
        MergeAttribute(context.Attributes, "data-val-emptywhen", GetErrorMessage(context));
        MergeAttribute(context.Attributes, "data-val-emptywhen-condition", "*." + Attribute.ConditionProperty);
        MergeAttribute(context.Attributes, "data-val-emptywhen-value", ToBoolean(Attribute.ConditionValue));
        MergeAttribute(context.Attributes, "data-val-emptywhen-allowempty", ToBoolean(Attribute.AllowEmptyStrings));
    }
}
