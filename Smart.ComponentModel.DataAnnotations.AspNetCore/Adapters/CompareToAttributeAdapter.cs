namespace Smart.ComponentModel.DataAnnotations.AspNetCore.Adapters;

using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.Extensions.Localization;

internal sealed class CompareToAttributeAdapter : AttributeAdapter<CompareToAttribute>
{
    public CompareToAttributeAdapter(CompareToAttribute attribute, IStringLocalizer? stringLocalizer, DefaultMessageLocalizer? localizer)
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
        MergeAttribute(context.Attributes, "data-val-compareto", GetErrorMessage(context));
        MergeAttribute(context.Attributes, "data-val-compareto-other", "*." + Attribute.OtherProperty);
        MergeAttribute(context.Attributes, "data-val-compareto-operation", Attribute.Operation.ToString());
        MergeAttribute(context.Attributes, "data-val-compareto-type", type);
    }
}
