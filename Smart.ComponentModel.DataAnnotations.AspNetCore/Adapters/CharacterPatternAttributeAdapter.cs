namespace Smart.ComponentModel.DataAnnotations.AspNetCore.Adapters;

using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.Extensions.Localization;

internal sealed class CharacterPatternAttributeAdapter : AttributeAdapter<CharacterPatternAttribute>
{
    public CharacterPatternAttributeAdapter(CharacterPatternAttribute attribute, IStringLocalizer? stringLocalizer, DefaultMessageLocalizer? localizer)
        : base(attribute, stringLocalizer, localizer)
    {
    }

    public override void AddValidation(ClientModelValidationContext context)
    {
        MergeAttribute(context.Attributes, "data-val", "true");
        MergeAttribute(context.Attributes, "data-val-charactertype", GetErrorMessage(context));
        MergeAttribute(context.Attributes, "data-val-charactertype-pattern", Attribute.Pattern);
    }
}
