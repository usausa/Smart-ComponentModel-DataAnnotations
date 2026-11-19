namespace Smart.ComponentModel.DataAnnotations;

using System.Text.RegularExpressions;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public sealed partial class NumericHyphenAttribute : CharacterPatternAttribute
{
    private const string PatternValue = "[0-9-]*";

    public override string Pattern => PatternValue;

    public NumericHyphenAttribute()
        : base(static () => JapaneseValidationMessages.NumericHyphen)
    {
    }

    protected override bool IsMatch(string value) =>
        IsFullMatch(PatternRegex(), value);

    [GeneratedRegex(PatternValue)]
    private static partial Regex PatternRegex();
}
