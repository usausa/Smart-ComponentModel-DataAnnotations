namespace Smart.ComponentModel.DataAnnotations;

using System.ComponentModel.DataAnnotations;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
public sealed class EmptyWhenAttribute : ConditionalValidationAttribute
{
    public bool AllowEmptyStrings { get; set; }

    public EmptyWhenAttribute(string conditionProperty)
        : base(conditionProperty, static () => ValidationMessages.Empty)
    {
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext) =>
        !ValidationHelper.HasValue(value, AllowEmptyStrings) || !IsConditionMet(validationContext.ObjectInstance)
            ? ValidationResult.Success
            : new ValidationResult(FormatErrorMessage(validationContext.DisplayName), validationContext.ToMemberNames());
}
