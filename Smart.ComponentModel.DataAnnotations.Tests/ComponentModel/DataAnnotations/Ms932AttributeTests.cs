namespace Smart.ComponentModel.DataAnnotations;

public sealed class Ms932AttributeTests
{
    // ------------------------------------------------------------
    // Length
    // ------------------------------------------------------------

    [Theory]
    [InlineData("abcd", true)]
    [InlineData("abcde", false)]
    [InlineData("\uFF71\uFF72\uFF73\uFF74", true)]
    [InlineData("\u3042\u3044", true)]
    [InlineData("\u3042\u3044\u3046", false)]
    public void MaxLengthCountsShiftJisBytes(string value, bool expected)
    {
        // Arrange
        var attribute = new Ms932MaxLengthAttribute(4);

        // Act
        var valid = attribute.IsValid(value);

        // Assert
        Assert.Equal(expected, valid);
    }

    [Theory]
    [InlineData("a", false)]
    [InlineData("ab", true)]
    [InlineData("\u3042\u3044", true)]
    [InlineData("abcde", false)]
    public void LengthChecksMinimumLength(string value, bool expected)
    {
        // Arrange
        var attribute = new Ms932LengthAttribute(2, 4);

        // Act
        var valid = attribute.IsValid(value);

        // Assert
        Assert.Equal(expected, valid);
    }

    [Theory]
    [InlineData("abc", false)]
    [InlineData("abcd", true)]
    [InlineData("\u3042\u3044", true)]
    [InlineData("abcde", false)]
    public void FixedLengthChecksLength(string value, bool expected)
    {
        // Arrange
        var attribute = new Ms932FixedLengthAttribute(4);

        // Act
        var valid = attribute.IsValid(value);

        // Assert
        Assert.Equal(expected, valid);
    }

    [Fact]
    public void LengthChecksEachElement()
    {
        // Arrange
        var attribute = new Ms932MaxLengthAttribute(4);

        // Act
        var valid = attribute.IsValid(new List<string?> { "ab", null });
        var invalid = attribute.IsValid(new List<string?> { "ab", "\u3042\u3044\u3046" });

        // Assert
        Assert.True(valid);
        Assert.False(invalid);
    }

    [Fact]
    public void NullIsValid()
    {
        // Arrange
        var maximum = new Ms932MaxLengthAttribute(4);
        var range = new Ms932LengthAttribute(2, 4);
        var encodable = new Ms932EncodableAttribute();

        // Act & Assert
        Assert.True(maximum.IsValid(null));
        Assert.True(range.IsValid(null));
        Assert.True(encodable.IsValid(null));
    }

    [Fact]
    public void FormatErrorMessageUsesLengths()
    {
        // Act
        var maximum = new Ms932MaxLengthAttribute(4).FormatErrorMessage("Name");
        var range = new Ms932LengthAttribute(2, 4).FormatErrorMessage("Name");
        var fixedLength = new Ms932FixedLengthAttribute(4).FormatErrorMessage("Name");

        // Assert
        Assert.Equal("The field Name must be a string with a maximum length of 4 bytes in Shift_JIS.", maximum);
        Assert.Equal("The field Name must be a string with a minimum length of 2 bytes and a maximum length of 4 bytes in Shift_JIS.", range);
        Assert.Equal("The field Name must be a string with a length of 4 bytes in Shift_JIS.", fixedLength);
    }

    [Fact]
    public void InvalidLengthsThrow()
    {
        // Arrange
        var maximum = new Ms932MaxLengthAttribute(-1);
        var range = new Ms932LengthAttribute(5, 4);
        var fixedLength = new Ms932FixedLengthAttribute(-1);

        // Act & Assert
        Assert.IsType<InvalidOperationException>(Record.Exception(() => maximum.IsValid("a")));
        Assert.IsType<InvalidOperationException>(Record.Exception(() => range.IsValid("a")));
        Assert.IsType<InvalidOperationException>(Record.Exception(() => fixedLength.IsValid("a")));
    }

    // ------------------------------------------------------------
    // Encodable
    // ------------------------------------------------------------

    [Theory]
    [InlineData("abc\uFF71\u3042", true)]
    [InlineData("\uFF5E\uFF0D\u2460\u9AD9", true)]
    [InlineData("\u301C", false)]
    [InlineData("\u2212", false)]
    [InlineData("\u2122", false)]
    [InlineData("\u00E9", false)]
    [InlineData("\uD842\uDFB7", false)]
    [InlineData("\uD83D\uDE00", false)]
    [InlineData("\u304B\u3099", false)]
    public void EncodableChecksCharacters(string value, bool expected)
    {
        // Arrange
        var attribute = new Ms932EncodableAttribute();

        // Act
        var valid = attribute.IsValid(value);

        // Assert
        Assert.Equal(expected, valid);
    }

    [Fact]
    public void EncodableChecksEachElement()
    {
        // Arrange
        var attribute = new Ms932EncodableAttribute();

        // Act
        var valid = attribute.IsValid(new List<string?> { "a", null });
        var invalid = attribute.IsValid(new List<string?> { "a", "\u301C" });

        // Assert
        Assert.True(valid);
        Assert.False(invalid);
    }

    [Fact]
    public void EncodableMessageUsesName()
    {
        // Act
        var message = new Ms932EncodableAttribute().FormatErrorMessage("Name");

        // Assert
        Assert.Equal("The field Name must contain only characters that can be encoded in Shift_JIS.", message);
    }
}
