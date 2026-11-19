namespace Smart.ComponentModel.DataAnnotations;

using System.ComponentModel.DataAnnotations;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
public sealed class RequiredWhenAttribute : ConditionalValidationAttribute
{
    public bool AllowEmptyStrings { get; set; }

    public RequiredWhenAttribute(string conditionProperty)
        : base(conditionProperty, static () => ValidationMessages.Required)
    {
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext) =>
        ValidationHelper.HasValue(value, AllowEmptyStrings) || !IsConditionMet(validationContext.ObjectInstance)
            ? ValidationResult.Success
            : new ValidationResult(FormatErrorMessage(validationContext.DisplayName), validationContext.ToMemberNames());
}
