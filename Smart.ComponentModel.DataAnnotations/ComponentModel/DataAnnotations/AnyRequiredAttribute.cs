namespace Smart.ComponentModel.DataAnnotations;

using System.ComponentModel.DataAnnotations;
using System.Globalization;

using Smart.ComponentModel.DataAnnotations.Internal;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true)]
public sealed class AnyRequiredAttribute : ValidationAttribute, IMemberMessageFormatter
{
    private readonly string[] properties;

    private Func<string>[]? displayNames;

    public IReadOnlyList<string> Properties => properties;

    public bool AllowEmptyStrings { get; set; }

    public override bool RequiresValidationContext => true;

    public override object TypeId => this;

    bool IMemberMessageFormatter.AppliesToType => true;

    IReadOnlyList<string> IMemberMessageFormatter.MessageMembers => properties;

    public AnyRequiredAttribute(params string[] properties)
        : base(static () => ValidationMessages.AnyRequired)
    {
        this.properties = properties;
    }

#if NET11_0_OR_GREATER
    public override string FormatMessage(string format, string name) =>
        Format(format, name);
#else
    public override string FormatErrorMessage(string name) =>
        Format(ErrorMessageString, name);
#endif

    string IMemberMessageFormatter.FormatMessage(string? format, string name, IReadOnlyList<string> memberDisplayNames) =>
        String.Format(CultureInfo.CurrentCulture, format ?? ErrorMessageString, name, String.Join(", ", memberDisplayNames));

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (properties.Length == 0)
        {
            throw new InvalidOperationException("Properties are not specified.");
        }

        if (value is null)
        {
            return ValidationResult.Success;
        }

        var type = value.GetType();
        foreach (var property in properties)
        {
            if (ValidationHelper.HasValue(ValidationRegistry.GetMember(type, property)(value), AllowEmptyStrings))
            {
                return ValidationResult.Success;
            }
        }

        displayNames ??= [.. properties.Select(x => ValidationHelper.ResolveDisplayName(type, x))];
        return new ValidationResult(FormatErrorMessage(validationContext.DisplayName), properties);
    }

    private string Format(string format, string name) =>
        String.Format(CultureInfo.CurrentCulture, format, name, displayNames is null ? String.Join(", ", properties) : String.Join(", ", displayNames.Select(static x => x())));
}
