namespace Smart.ComponentModel.DataAnnotations;

using System.ComponentModel.DataAnnotations;
using System.Globalization;

using Smart.ComponentModel.DataAnnotations.Internal;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
public sealed class CompareToWhenAttribute : ConditionalValidationAttribute, IMemberMessageFormatter
{
    private Func<string>? otherDisplayName;

    public CompareToOperation Operation { get; }

    public string OtherProperty { get; }

    bool IMemberMessageFormatter.AppliesToType => false;

    IReadOnlyList<string> IMemberMessageFormatter.MessageMembers => [OtherProperty];

    public CompareToWhenAttribute(string conditionProperty, CompareToOperation operation, string otherProperty)
        : base(conditionProperty, () => operation.ResolveMessage())
    {
        Operation = operation;
        OtherProperty = otherProperty;
    }

#if NET11_0_OR_GREATER
    public override string FormatMessage(string format, string name) =>
        Format(format, name);
#else
    public override string FormatErrorMessage(string name) =>
        Format(ErrorMessageString, name);
#endif

    string IMemberMessageFormatter.FormatMessage(string? format, string name, IReadOnlyList<string> memberDisplayNames) =>
        String.Format(CultureInfo.CurrentCulture, format ?? ErrorMessageString, name, memberDisplayNames[0]);

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var instance = validationContext.ObjectInstance;
        if (!IsConditionMet(instance))
        {
            return ValidationResult.Success;
        }

        var type = instance.GetType();
        var other = ValidationRegistry.GetMember(type, OtherProperty)(instance);
        if ((value is null) || (other is null) || Operation.IsValid(ValidationRegistry.GetComparer(value.GetType())(value, other)))
        {
            return ValidationResult.Success;
        }

        otherDisplayName ??= ValidationHelper.ResolveDisplayName(type, OtherProperty);
        return new ValidationResult(FormatErrorMessage(validationContext.DisplayName), validationContext.ToMemberNames());
    }

    private string Format(string format, string name) =>
        String.Format(CultureInfo.CurrentCulture, format, name, otherDisplayName?.Invoke() ?? OtherProperty);
}
