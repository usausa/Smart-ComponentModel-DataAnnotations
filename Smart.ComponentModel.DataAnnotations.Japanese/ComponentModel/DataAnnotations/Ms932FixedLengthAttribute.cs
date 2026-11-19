namespace Smart.ComponentModel.DataAnnotations;

using System.ComponentModel.DataAnnotations;
using System.Globalization;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public sealed class Ms932FixedLengthAttribute : ValidationAttribute
{
    public int Length { get; }

    public Ms932FixedLengthAttribute(int length)
        : base(static () => JapaneseValidationMessages.Ms932FixedLength)
    {
        Length = length;
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
        if (Length < 0)
        {
            throw new InvalidOperationException($"Invalid length. length=[{Length}]");
        }

        return Ms932.IsValidLength(value, Length, Length);
    }

    private string Format(string format, string name) =>
        String.Format(CultureInfo.CurrentCulture, format, name, Length);
}
