namespace Smart.ComponentModel.DataAnnotations.AspNetCore;

using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

internal sealed class LocalizedModelValidatorProvider : IMetadataBasedModelValidatorProvider
{
    private readonly DefaultMessageLocalizer localizer;

    public LocalizedModelValidatorProvider(DefaultMessageLocalizer localizer)
    {
        this.localizer = localizer;
    }

    public void CreateValidators(ModelValidatorProviderContext context)
    {
        foreach (var item in context.Results)
        {
            if ((item.ValidatorMetadata is ValidationAttribute attribute) && (item.Validator is { } validator and not LocalizedModelValidator))
            {
                item.Validator = new LocalizedModelValidator(validator, attribute, localizer);
            }
        }
    }

    public bool HasValidators(Type modelType, IList<object> validatorMetadata) => false;
}
