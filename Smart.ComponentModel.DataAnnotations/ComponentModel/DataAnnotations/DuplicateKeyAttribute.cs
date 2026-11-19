namespace Smart.ComponentModel.DataAnnotations;

using System.ComponentModel.DataAnnotations;
using System.Globalization;

using Smart.ComponentModel.DataAnnotations.Internal;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter, AllowMultiple = true)]
public sealed class DuplicateKeyAttribute : ValidationAttribute
{
    private readonly string[] members;

    private readonly string keys;

    public IReadOnlyList<string> Members => members;

    public override bool RequiresValidationContext => true;

    public override object TypeId => this;

    public DuplicateKeyAttribute(params string[] members)
        : base(static () => ValidationMessages.DuplicateKey)
    {
        this.members = members;
        keys = String.Join(",", members);
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
        if (members.Length == 0)
        {
            throw new InvalidOperationException("Members are not specified.");
        }

        if (value is null)
        {
            return ValidationResult.Success;
        }

        var instance = validationContext.ObjectInstance;
        return ValidationRegistry.GetDuplicate(instance.GetType(), validationContext.GetMemberName(), keys)(instance)
            ? new ValidationResult(FormatErrorMessage(validationContext.DisplayName), validationContext.ToMemberNames())
            : ValidationResult.Success;
    }

    private string Format(string format, string name) =>
        String.Format(CultureInfo.CurrentCulture, format, name, String.Join(", ", members));
}
