namespace Smart.ComponentModel.DataAnnotations;

using System.Text.RegularExpressions;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public sealed partial class NoHalfWidthKanaAttribute : CharacterPatternAttribute
{
    private const string PatternValue = @"[^\uFF61-\uFF9F]*";

    public override string Pattern => PatternValue;

    public NoHalfWidthKanaAttribute()
        : base(static () => JapaneseValidationMessages.NoHalfWidthKana)
    {
    }

    protected override bool IsMatch(string value) =>
        IsFullMatch(PatternRegex(), value);

    [GeneratedRegex(PatternValue)]
    private static partial Regex PatternRegex();
}
