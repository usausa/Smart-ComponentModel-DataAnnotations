namespace Smart.ComponentModel.DataAnnotations;

using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

public abstract class CharacterPatternAttribute : ValidationAttribute
{
    public abstract string Pattern { get; }

    protected CharacterPatternAttribute(Func<string> errorMessageAccessor)
        : base(errorMessageAccessor)
    {
    }

    public override bool IsValid(object? value)
    {
        switch (value)
        {
            case string str:
                return IsValidValue(str);
            case IEnumerable<string?> values:
                foreach (var element in values)
                {
                    if (!IsValidValue(element))
                    {
                        return false;
                    }
                }

                return true;
            default:
                return true;
        }
    }

    protected abstract bool IsMatch(string value);

    protected static bool IsFullMatch(Regex regex, string value)
    {
        var match = regex.Match(value);
        return match.Success && (match.Index == 0) && (match.Length == value.Length);
    }

    private bool IsValidValue(string? value) =>
        String.IsNullOrEmpty(value) || IsMatch(value);
}
