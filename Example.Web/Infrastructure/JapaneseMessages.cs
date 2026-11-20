namespace Example.Web.Infrastructure;

using System.Globalization;

using Microsoft.Extensions.Localization;

internal static class JapaneseMessages
{
    private static readonly Dictionary<string, string> Messages = new()
    {
        ["RequiredAttribute_Error"] = "{0}を入力してください",
        ["StringLengthAttribute_Error"] = "{0}は{1}文字以内で入力してください",
        ["RangeAttribute_Error"] = "{0}は{1}から{2}の範囲で入力してください",
        ["EmailAddressAttribute_Error"] = "{0}はメールアドレスの形式で入力してください",
        ["RequiredWhenAttribute_Error"] = "{0}を入力してください",
        ["EmptyWhenAttribute_Error"] = "{0}は入力しないでください",
        ["CompareToAttribute_Error"] = "{0}と{1}の大小が正しくありません",
        ["CompareToWhenAttribute_Error"] = "{0}と{1}の大小が正しくありません",
        ["EqualToAttribute_Error"] = "{0}が{1}と一致しません",
        ["AnyRequiredAttribute_Error"] = "{1}のいずれかを入力してください",
        ["Ms932MaxLengthAttribute_Error"] = "{0}は{1}バイト以内で入力してください",
        ["Ms932FixedLengthAttribute_Error"] = "{0}は{1}バイトで入力してください",
        ["FullWidthAttribute_Error"] = "{0}は全角で入力してください",
        ["KatakanaAttribute_Error"] = "{0}は全角カタカナで入力してください",
        ["UpperAlphanumericAttribute_Error"] = "{0}は半角英大文字と数字で入力してください",
        ["NumericHyphenAttribute_Error"] = "{0}は半角数字とハイフンで入力してください",
        ["AttemptedValueIsInvalid_Error"] = "{1}の形式が正しくありません",
        ["ValueMustBeANumber_Error"] = "{0}は数値で入力してください",
        ["ValueMustNotBeNull_Error"] = "入力してください"
    };

    public static IStringLocalizer Localizer { get; } = new DictionaryStringLocalizer(Messages);

    private sealed class DictionaryStringLocalizer : IStringLocalizer
    {
        private readonly Dictionary<string, string> values;

        public DictionaryStringLocalizer(Dictionary<string, string> values)
        {
            this.values = values;
        }

        public LocalizedString this[string name] =>
            values.TryGetValue(name, out var value) ? new LocalizedString(name, value) : new LocalizedString(name, name, resourceNotFound: true);

        public LocalizedString this[string name, params object[] arguments]
        {
            get
            {
                var value = this[name];
                return new LocalizedString(name, String.Format(CultureInfo.CurrentCulture, value.Value, arguments), value.ResourceNotFound);
            }
        }

        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) =>
            values.Select(static x => new LocalizedString(x.Key, x.Value));
    }
}
