namespace Smart.ComponentModel.DataAnnotations;

using System.ComponentModel.DataAnnotations;
using System.Globalization;

using Smart.ComponentModel.DataAnnotations.Internal;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true)]
public sealed class AnyRequiredWhenAttribute : ConditionalValidationAttribute
{
    private readonly string[] properties;

    private string? displayNames;

    public IReadOnlyList<string> Properties => properties;

    public bool AllowEmptyStrings { get; set; }

    public override object TypeId => this;

    public AnyRequiredWhenAttribute(string conditionProperty, params string[] properties)
        : base(conditionProperty, static () => ValidationMessages.AnyRequired)
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

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (properties.Length == 0)
        {
            throw new InvalidOperationException("Properties are not specified.");
        }

        if ((value is null) || !IsConditionMet(value))
        {
            return ValidationResult.Success;
        }

        var type = value.GetType();
        foreach (var property in properties)
        {
            if (ValidationRegistry.GetPresence(type, property)(value, AllowEmptyStrings))
            {
                return ValidationResult.Success;
            }
        }

        displayNames ??= String.Join(", ", properties.Select(x => ValidationHelper.ResolveDisplayName(type, x)));
        return new ValidationResult(FormatErrorMessage(validationContext.DisplayName), properties);
    }

    private string Format(string format, string name) =>
        String.Format(CultureInfo.CurrentCulture, format, name, displayNames ?? String.Join(", ", properties));
}
