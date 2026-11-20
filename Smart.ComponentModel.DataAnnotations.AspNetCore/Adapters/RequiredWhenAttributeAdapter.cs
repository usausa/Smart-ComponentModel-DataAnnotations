namespace Smart.ComponentModel.DataAnnotations.AspNetCore.Adapters;

using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.Extensions.Localization;

internal sealed class RequiredWhenAttributeAdapter : AttributeAdapter<RequiredWhenAttribute>
{
    public RequiredWhenAttributeAdapter(RequiredWhenAttribute attribute, IStringLocalizer? stringLocalizer, DefaultMessageLocalizer? localizer)
        : base(attribute, stringLocalizer, localizer)
    {
    }

    public override void AddValidation(ClientModelValidationContext context)
    {
        MergeAttribute(context.Attributes, "data-val", "true");
        MergeAttribute(context.Attributes, "data-val-requiredwhen", GetErrorMessage(context));
        MergeAttribute(context.Attributes, "data-val-requiredwhen-condition", "*." + Attribute.ConditionProperty);
        MergeAttribute(context.Attributes, "data-val-requiredwhen-value", ToBoolean(Attribute.ConditionValue));
        MergeAttribute(context.Attributes, "data-val-requiredwhen-allowempty", ToBoolean(Attribute.AllowEmptyStrings));
    }
}
