namespace Smart.ComponentModel.DataAnnotations;

using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public sealed class MaxCountAttribute : ValidationAttribute
{
    public int Count { get; }

    public MaxCountAttribute(int count)
        : base(static () => ValidationMessages.MaxCount)
    {
        Count = count;
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
        if (Count < 0)
        {
            throw new InvalidOperationException($"Invalid count. count=[{Count}]");
        }

        return value switch
        {
            string => true,
            ICollection collection => collection.Count <= Count,
            IEnumerable values => CountElements(values) <= Count,
            _ => true
        };
    }

    private int CountElements(IEnumerable values)
    {
        var count = 0;
        foreach (var _ in values)
        {
            count++;
            if (count > Count)
            {
                break;
            }
        }

        return count;
    }

    private string Format(string format, string name) =>
        String.Format(CultureInfo.CurrentCulture, format, name, Count);
}
