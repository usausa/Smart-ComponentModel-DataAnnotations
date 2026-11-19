namespace Smart.ComponentModel.DataAnnotations;

using System.ComponentModel.DataAnnotations;
using System.Globalization;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public sealed class Ms932LengthAttribute : ValidationAttribute
{
    public int MinimumLength { get; }

    public int MaximumLength { get; }

    public Ms932LengthAttribute(int minimumLength, int maximumLength)
        : base(static () => JapaneseValidationMessages.Ms932Length)
    {
        MinimumLength = minimumLength;
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
        if ((MinimumLength < 0) || (MaximumLength < MinimumLength))
        {
            throw new InvalidOperationException($"Invalid length. minimum=[{MinimumLength}], maximum=[{MaximumLength}]");
        }

        return Ms932.IsValidLength(value, MinimumLength, MaximumLength);
    }

    private string Format(string format, string name) =>
        String.Format(CultureInfo.CurrentCulture, format, name, MinimumLength, MaximumLength);
}
