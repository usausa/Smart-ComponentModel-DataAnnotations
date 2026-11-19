namespace Smart.ComponentModel.DataAnnotations;

using System.Text.RegularExpressions;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public sealed partial class KatakanaAttribute : CharacterPatternAttribute
{
    private const string PatternValue = @"[\u30A1-\u30F6\u30FC]*";

    private const string SpacePatternValue = @"[\u30A1-\u30F6\u30FC\u3000 ]*";

    public bool AllowSpace { get; set; }

    public override string Pattern => AllowSpace ? SpacePatternValue : PatternValue;

    public KatakanaAttribute()
        : base(static () => JapaneseValidationMessages.Katakana)
    {
    }

    protected override bool IsMatch(string value) =>
        IsFullMatch(AllowSpace ? SpacePatternRegex() : PatternRegex(), value);

    [GeneratedRegex(PatternValue)]
    private static partial Regex PatternRegex();

    [GeneratedRegex(SpacePatternValue)]
    private static partial Regex SpacePatternRegex();
}
