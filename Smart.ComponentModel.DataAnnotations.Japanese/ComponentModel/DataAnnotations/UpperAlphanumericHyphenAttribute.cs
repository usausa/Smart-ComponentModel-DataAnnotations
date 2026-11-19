namespace Smart.ComponentModel.DataAnnotations;

using System.Text.RegularExpressions;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public sealed partial class UpperAlphanumericHyphenAttribute : CharacterPatternAttribute
{
    private const string PatternValue = "[A-Z0-9-]*";

    public override string Pattern => PatternValue;

    public UpperAlphanumericHyphenAttribute()
        : base(static () => JapaneseValidationMessages.UpperAlphanumericHyphen)
    {
    }

    protected override bool IsMatch(string value) =>
        IsFullMatch(PatternRegex(), value);

    [GeneratedRegex(PatternValue)]
    private static partial Regex PatternRegex();
}
