namespace Smart.ComponentModel.DataAnnotations;

public sealed class CharacterPatternAttributeTests
{
    [Theory]
    [InlineData("NoHalfWidthKana", "\u3042\u3044\u3046ABC", true)]
    [InlineData("NoHalfWidthKana", "\uFF71\uFF72\uFF73", false)]
    [InlineData("NoHalfWidthKana", "\u3042\uFF61", false)]
    [InlineData("FullWidth", "\u3042\u3044\u3046\uFF21\uFF22\uFF23", true)]
    [InlineData("FullWidth", "\u2460\u9AD9\uFF5E", true)]
    [InlineData("FullWidth", "\u3042\u3044\u3046A", false)]
    [InlineData("FullWidth", "\u3042 \u3044", false)]
    [InlineData("FullWidth", "\u3042\uFF71", false)]
    [InlineData("FullWidth", "\u00E9", false)]
    [InlineData("FullWidth", "\u301C", false)]
    [InlineData("FullWidth", "\uD83D\uDE00", false)]
    [InlineData("UpperAlphanumeric", "ABC123", true)]
    [InlineData("UpperAlphanumeric", "ABc123", false)]
    [InlineData("UpperAlphanumeric", "AB-12", false)]
    [InlineData("UpperAlphanumericHyphen", "AB-12", true)]
    [InlineData("UpperAlphanumericHyphen", "ab-12", false)]
    [InlineData("Alphanumeric", "abcXYZ09", true)]
    [InlineData("Alphanumeric", "abc-09", false)]
    [InlineData("AlphanumericHyphen", "abc-XYZ-09", true)]
    [InlineData("AlphanumericHyphen", "abc_09", false)]
    [InlineData("Numeric", "0123", true)]
    [InlineData("Numeric", "12a", false)]
    [InlineData("Numeric", "\uFF11\uFF12", false)]
    [InlineData("NumericHyphen", "03-1234-5678", true)]
    [InlineData("NumericHyphen", "03 1234", false)]
    [InlineData("Katakana", "\u30AB\u30BF\u30AB\u30CA\u30FC", true)]
    [InlineData("Katakana", "\u30F4\u30F5\u30F6", true)]
    [InlineData("Katakana", "\u30AB\u30BF \u30AB\u30CA", false)]
    [InlineData("Katakana", "\u304B\u305F\u304B\u306A", false)]
    [InlineData("Katakana", "\uFF76\uFF80\uFF76\uFF85", false)]
    [InlineData("KatakanaSpace", "\u30E4\u30DE\u30C0\u3000\u30BF\u30ED\u30A6", true)]
    [InlineData("KatakanaSpace", "\u30E4\u30DE\u30C0 \u30BF\u30ED\u30A6", true)]
    [InlineData("Hiragana", "\u3072\u3089\u304C\u306A\u30FC", true)]
    [InlineData("Hiragana", "\u3072\u3089\u304C\u306A\u3000\u3072", false)]
    [InlineData("Hiragana", "\u30AB\u30BF\u30AB\u30CA", false)]
    [InlineData("HiraganaSpace", "\u3084\u307E\u3060\u3000\u305F\u308D\u3046", true)]
    public void IsValidChecksCharacters(string name, string value, bool expected)
    {
        // Arrange
        var attribute = Create(name);

        // Act
        var valid = attribute.IsValid(value);

        // Assert
        Assert.Equal(expected, valid);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void EmptyIsValid(string? value)
    {
        // Arrange
        var attribute = new NumericAttribute();

        // Act
        var valid = attribute.IsValid(value);

        // Assert
        Assert.True(valid);
    }

    [Fact]
    public void TrailingNewLineIsInvalid()
    {
        // Arrange
        var attribute = new NumericAttribute();

        // Act
        var valid = attribute.IsValid("123" + Environment.NewLine);

        // Assert
        Assert.False(valid);
    }

    [Fact]
    public void IsValidChecksEachElement()
    {
        // Arrange
        var attribute = new NumericAttribute();

        // Act
        var valid = attribute.IsValid(new List<string?> { "12", null, string.Empty });
        var invalid = attribute.IsValid(new List<string?> { "12", "ab" });

        // Assert
        Assert.True(valid);
        Assert.False(invalid);
    }

    [Fact]
    public void FormatErrorMessageDependsOnAttribute()
    {
        // Act
        var numeric = new NumericAttribute().FormatErrorMessage("Code");
        var fullWidth = new FullWidthAttribute().FormatErrorMessage("Name");
        var katakana = new KatakanaAttribute().FormatErrorMessage("Kana");

        // Assert
        Assert.Equal("The field Code must contain only digits.", numeric);
        Assert.Equal("The field Name must contain only full-width characters.", fullWidth);
        Assert.Equal("The field Kana must contain only full-width katakana.", katakana);
    }

    [Fact]
    public void PatternIsForClient()
    {
        // Act
        var numeric = new NumericHyphenAttribute().Pattern;
        var katakana = new KatakanaAttribute().Pattern;
        var katakanaSpace = new KatakanaAttribute { AllowSpace = true }.Pattern;

        // Assert
        Assert.Equal("[0-9-]*", numeric);
        Assert.Equal(@"[\u30A1-\u30F6\u30FC]*", katakana);
        Assert.Equal(@"[\u30A1-\u30F6\u30FC\u3000 ]*", katakanaSpace);
    }

    private static CharacterPatternAttribute Create(string name) =>
        name switch
        {
            "NoHalfWidthKana" => new NoHalfWidthKanaAttribute(),
            "FullWidth" => new FullWidthAttribute(),
            "UpperAlphanumeric" => new UpperAlphanumericAttribute(),
            "UpperAlphanumericHyphen" => new UpperAlphanumericHyphenAttribute(),
            "Alphanumeric" => new AlphanumericAttribute(),
            "AlphanumericHyphen" => new AlphanumericHyphenAttribute(),
            "Numeric" => new NumericAttribute(),
            "NumericHyphen" => new NumericHyphenAttribute(),
            "Katakana" => new KatakanaAttribute(),
            "KatakanaSpace" => new KatakanaAttribute { AllowSpace = true },
            "Hiragana" => new HiraganaAttribute(),
            "HiraganaSpace" => new HiraganaAttribute { AllowSpace = true },
            _ => throw new ArgumentOutOfRangeException(nameof(name))
        };
}
