namespace Smart.ComponentModel.DataAnnotations.AspNetCore;

using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Mvc.DataAnnotations;
using Microsoft.Extensions.Localization;

using Smart.ComponentModel.DataAnnotations.AspNetCore.Adapters;

internal sealed class AttributeAdapterProvider : IValidationAttributeAdapterProvider
{
    private readonly ValidationAttributeAdapterProvider provider = new();

    private readonly DefaultMessageLocalizer? localizer;

    public AttributeAdapterProvider(DefaultMessageLocalizer? localizer)
    {
        this.localizer = localizer;
    }

    public IAttributeAdapter? GetAttributeAdapter(ValidationAttribute attribute, IStringLocalizer? stringLocalizer) =>
        attribute switch
        {
            CompareToAttribute compareTo => new CompareToAttributeAdapter(compareTo, stringLocalizer, localizer),
            CompareToWhenAttribute compareToWhen => new CompareToWhenAttributeAdapter(compareToWhen, stringLocalizer, localizer),
            EqualToAttribute equalTo => new EqualToAttributeAdapter(equalTo, stringLocalizer, localizer),
            RequiredWhenAttribute requiredWhen => new RequiredWhenAttributeAdapter(requiredWhen, stringLocalizer, localizer),
            EmptyWhenAttribute emptyWhen => new EmptyWhenAttributeAdapter(emptyWhen, stringLocalizer, localizer),
            Ms932MaxLengthAttribute maxLength => new Ms932LengthAttributeAdapter<Ms932MaxLengthAttribute>(maxLength, 0, maxLength.MaximumLength, [maxLength.MaximumLength], stringLocalizer, localizer),
            Ms932LengthAttribute length => new Ms932LengthAttributeAdapter<Ms932LengthAttribute>(length, length.MinimumLength, length.MaximumLength, [length.MinimumLength, length.MaximumLength], stringLocalizer, localizer),
            Ms932FixedLengthAttribute fixedLength => new Ms932LengthAttributeAdapter<Ms932FixedLengthAttribute>(fixedLength, fixedLength.Length, fixedLength.Length, [fixedLength.Length], stringLocalizer, localizer),
            CharacterPatternAttribute pattern => new CharacterPatternAttributeAdapter(pattern, stringLocalizer, localizer),
            _ => Wrap(provider.GetAttributeAdapter(attribute, stringLocalizer), attribute)
        };

    private IAttributeAdapter? Wrap(IAttributeAdapter? adapter, ValidationAttribute attribute) =>
        (adapter is not null) && (localizer is not null) ? new LocalizedAttributeAdapter(adapter, attribute, localizer) : adapter;
}
