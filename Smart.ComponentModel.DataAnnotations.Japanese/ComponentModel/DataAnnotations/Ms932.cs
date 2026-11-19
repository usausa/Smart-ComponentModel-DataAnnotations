namespace Smart.ComponentModel.DataAnnotations;

using System.Text;

internal static class Ms932
{
    private static readonly Encoding Encoding = CodePagesEncodingProvider.Instance.GetEncoding(932)!;

    private static readonly Encoding StrictEncoding = CodePagesEncodingProvider.Instance.GetEncoding(932, EncoderFallback.ExceptionFallback, DecoderFallback.ExceptionFallback)!;

    public static bool IsValidLength(object? value, int minimumLength, int maximumLength)
    {
        switch (value)
        {
            case string str:
                return IsValidLength(str, minimumLength, maximumLength);
            case IEnumerable<string?> values:
                foreach (var element in values)
                {
                    if ((element is not null) && !IsValidLength(element, minimumLength, maximumLength))
                    {
                        return false;
                    }
                }

                return true;
            default:
                return true;
        }
    }

    public static bool IsEncodable(string value) =>
        TryGetStrictByteCount(value, out _);

    public static bool IsFullWidth(string value) =>
        TryGetStrictByteCount(value, out var count) && (count == value.Length * 2);

    private static bool IsValidLength(string value, int minimumLength, int maximumLength)
    {
        var length = Encoding.GetByteCount(value);
        return (length >= minimumLength) && (length <= maximumLength);
    }

    private static bool TryGetStrictByteCount(string value, out int count)
    {
        try
        {
            count = StrictEncoding.GetByteCount(value);
            return true;
        }
        catch (EncoderFallbackException)
        {
            count = 0;
            return false;
        }
    }
}
