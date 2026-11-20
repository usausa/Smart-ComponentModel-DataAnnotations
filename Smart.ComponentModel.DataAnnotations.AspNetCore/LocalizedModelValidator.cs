namespace Smart.ComponentModel.DataAnnotations.AspNetCore;

using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

internal sealed class LocalizedModelValidator : IModelValidator
{
    private readonly IModelValidator validator;

    private readonly ValidationAttribute attribute;

    private readonly DefaultMessageLocalizer localizer;

    public LocalizedModelValidator(IModelValidator validator, ValidationAttribute attribute, DefaultMessageLocalizer localizer)
    {
        this.validator = validator;
        this.attribute = attribute;
        this.localizer = localizer;
    }

    public IEnumerable<ModelValidationResult> Validate(ModelValidationContext context)
    {
        var results = validator.Validate(context).ToList();
        if ((results.Count == 0) || (ValidationMessageResolver.FormatMessage(attribute, context, localizer) is not { } message))
        {
            return results;
        }

        return [.. results.Select(x => new ModelValidationResult(x.MemberName, message))];
    }
}
