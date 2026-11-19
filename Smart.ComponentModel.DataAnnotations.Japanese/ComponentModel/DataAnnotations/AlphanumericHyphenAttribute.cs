namespace Smart.ComponentModel.DataAnnotations;

using System.Text.RegularExpressions;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public sealed partial class AlphanumericHyphenAttribute : CharacterPatternAttribute
{
    private const string PatternValue = "[a-zA-Z0-9-]*";

    public override string Pattern => PatternValue;

    public AlphanumericHyphenAttribute()
        : base(static () => JapaneseValidationMessages.AlphanumericHyphen)
    {
    }

    protected override bool IsMatch(string value) =>
        IsFullMatch(PatternRegex(), value);

    [GeneratedRegex(PatternValue)]
    private static partial Regex PatternRegex();
}
