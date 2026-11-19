namespace Smart.ComponentModel.DataAnnotations;

internal static class JapaneseValidationMessages
{
    public const string Ms932MaxLength = "The field {0} must be a string with a maximum length of {1} bytes in Shift_JIS.";

    public const string Ms932Length = "The field {0} must be a string with a minimum length of {1} bytes and a maximum length of {2} bytes in Shift_JIS.";

    public const string Ms932FixedLength = "The field {0} must be a string with a length of {1} bytes in Shift_JIS.";

    public const string Ms932Encodable = "The field {0} must contain only characters that can be encoded in Shift_JIS.";

    public const string NoHalfWidthKana = "The field {0} must not contain half-width katakana.";

    public const string FullWidth = "The field {0} must contain only full-width characters.";

    public const string UpperAlphanumeric = "The field {0} must contain only uppercase letters and digits.";

    public const string UpperAlphanumericHyphen = "The field {0} must contain only uppercase letters, digits, and hyphens.";

    public const string Alphanumeric = "The field {0} must contain only letters and digits.";

    public const string AlphanumericHyphen = "The field {0} must contain only letters, digits, and hyphens.";

    public const string Numeric = "The field {0} must contain only digits.";

    public const string NumericHyphen = "The field {0} must contain only digits and hyphens.";

    public const string Katakana = "The field {0} must contain only full-width katakana.";

    public const string Hiragana = "The field {0} must contain only hiragana.";
}
