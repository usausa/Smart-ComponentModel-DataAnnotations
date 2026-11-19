namespace Smart.ComponentModel.DataAnnotations;

using System.ComponentModel.DataAnnotations;
using System.Globalization;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public sealed class Ms932MaxLengthAttribute : ValidationAttribute
{
    public int MaximumLength { get; }

    public Ms932MaxLengthAttribute(int maximumLength)
        : base(static () => JapaneseValidationMessages.Ms932MaxLength)
    {
        MaximumLength = maximumLength;
    }

#if NET11_0_OR_GREATER
    public override string FormatMessage(string format, string name) =>
        Format(format, name);
#else
    public override string FormatErrorMessage(string name) =>
        Format(ErrorMessageString, name);
#endif

    public override bool IsValid(object? value)
    {
        if (MaximumLength < 0)
        {
            throw new InvalidOperationException($"Invalid length. maximum=[{MaximumLength}]");
        }

        return Ms932.IsValidLength(value, 0, MaximumLength);
    }

    private string Format(string format, string name) =>
        String.Format(CultureInfo.CurrentCulture, format, name, MaximumLength);
}
