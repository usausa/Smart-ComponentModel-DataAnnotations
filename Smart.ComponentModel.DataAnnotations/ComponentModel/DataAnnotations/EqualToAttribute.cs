namespace Smart.ComponentModel.DataAnnotations;

using System.ComponentModel.DataAnnotations;
using System.Globalization;

using Smart.ComponentModel.DataAnnotations.Internal;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
public sealed class EqualToAttribute : ValidationAttribute, IMemberMessageFormatter
{
    private readonly string[] messageMembers;

    private Func<string>? otherDisplayName;

    public string OtherProperty { get; }

    public override bool RequiresValidationContext => true;

    bool IMemberMessageFormatter.AppliesToType => false;

    IReadOnlyList<string> IMemberMessageFormatter.MessageMembers => messageMembers;

    public EqualToAttribute(string otherProperty)
        : base(static () => ValidationMessages.EqualTo)
    {
        OtherProperty = otherProperty;
        messageMembers = [otherProperty];
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
        var type = instance.GetType();
        if (Equals(value, ValidationRegistry.GetMember(type, OtherProperty)(instance)))
        {
            return ValidationResult.Success;
        }

        otherDisplayName ??= ValidationHelper.ResolveDisplayName(type, OtherProperty);
        return new ValidationResult(FormatErrorMessage(validationContext.DisplayName), validationContext.ToMemberNames());
    }

    private string Format(string format, string name) =>
        String.Format(CultureInfo.CurrentCulture, format, name, otherDisplayName?.Invoke() ?? OtherProperty);
}
