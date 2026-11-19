namespace Smart.ComponentModel.DataAnnotations;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public sealed class FullWidthAttribute : CharacterPatternAttribute
{
    private const string PatternValue = @"[^\u0000-\u007F\uFF61-\uFF9F]*";

    public override string Pattern => PatternValue;

    public FullWidthAttribute()
        : base(static () => JapaneseValidationMessages.FullWidth)
    {
    }

    protected override bool IsMatch(string value) =>
        Ms932.IsFullWidth(value);
}
