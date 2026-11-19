namespace Smart.ComponentModel.DataAnnotations;

using System.Text.RegularExpressions;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public sealed partial class HiraganaAttribute : CharacterPatternAttribute
{
    private const string PatternValue = @"[\u3041-\u3096\u30FC]*";

    private const string SpacePatternValue = @"[\u3041-\u3096\u30FC\u3000 ]*";

    public bool AllowSpace { get; set; }

    public override string Pattern => AllowSpace ? SpacePatternValue : PatternValue;

    public HiraganaAttribute()
        : base(static () => JapaneseValidationMessages.Hiragana)
    {
    }

    protected override bool IsMatch(string value) =>
        IsFullMatch(AllowSpace ? SpacePatternRegex() : PatternRegex(), value);

    [GeneratedRegex(PatternValue)]
    private static partial Regex PatternRegex();

    [GeneratedRegex(SpacePatternValue)]
    private static partial Regex SpacePatternRegex();
}
