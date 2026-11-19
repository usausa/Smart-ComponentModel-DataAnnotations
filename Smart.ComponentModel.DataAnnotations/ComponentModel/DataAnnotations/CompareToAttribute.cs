namespace Smart.ComponentModel.DataAnnotations;

using System.ComponentModel.DataAnnotations;
using System.Globalization;

using Smart.ComponentModel.DataAnnotations.Internal;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
public sealed class CompareToAttribute : ValidationAttribute
{
    private string? otherDisplayName;

    public CompareToOperation Operation { get; }

    public string OtherProperty { get; }

    public override bool RequiresValidationContext => true;

    public CompareToAttribute(CompareToOperation operation, string otherProperty)
        : base(() => operation.ResolveMessage())
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

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var instance = validationContext.ObjectInstance;
        var type = instance.GetType();
        if ((ValidationRegistry.GetComparison(type, validationContext.GetMemberName(), OtherProperty)(instance) is not { } compare) ||
            Operation.IsValid(compare))
        {
            return ValidationResult.Success;
        }

        otherDisplayName ??= ValidationHelper.ResolveDisplayName(type, OtherProperty);
        return new ValidationResult(FormatErrorMessage(validationContext.DisplayName), validationContext.ToMemberNames());
    }

    private string Format(string format, string name) =>
        String.Format(CultureInfo.CurrentCulture, format, name, otherDisplayName ?? OtherProperty);
}
