namespace Smart.ComponentModel.DataAnnotations.AspNetCore.Adapters;

using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.Extensions.Localization;

internal sealed class CompareToWhenAttributeAdapter : AttributeAdapter<CompareToWhenAttribute>
{
    public CompareToWhenAttributeAdapter(CompareToWhenAttribute attribute, IStringLocalizer? stringLocalizer, DefaultMessageLocalizer? localizer)
        : base(attribute, stringLocalizer, localizer)
    {
    }

    public override void AddValidation(ClientModelValidationContext context)
    {
        if (ClientValueType.Resolve(context.ModelMetadata) is not { } type)
        {
            return;
        }

        MergeAttribute(context.Attributes, "data-val", "true");
        MergeAttribute(context.Attributes, "data-val-comparetowhen", GetErrorMessage(context));
        MergeAttribute(context.Attributes, "data-val-comparetowhen-condition", "*." + Attribute.ConditionProperty);
        MergeAttribute(context.Attributes, "data-val-comparetowhen-value", ToBoolean(Attribute.ConditionValue));
        MergeAttribute(context.Attributes, "data-val-comparetowhen-other", "*." + Attribute.OtherProperty);
        MergeAttribute(context.Attributes, "data-val-comparetowhen-operation", Attribute.Operation.ToString());
        MergeAttribute(context.Attributes, "data-val-comparetowhen-type", type);
    }
}
