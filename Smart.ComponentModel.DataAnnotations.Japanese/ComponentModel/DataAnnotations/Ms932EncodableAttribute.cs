namespace Smart.ComponentModel.DataAnnotations;

using System.ComponentModel.DataAnnotations;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public sealed class Ms932EncodableAttribute : ValidationAttribute
{
    public Ms932EncodableAttribute()
        : base(static () => JapaneseValidationMessages.Ms932Encodable)
    {
    }

    public override bool IsValid(object? value)
    {
        switch (value)
        {
            case string str:
                return Ms932.IsEncodable(str);
            case IEnumerable<string?> values:
                foreach (var element in values)
                {
                    if ((element is not null) && !Ms932.IsEncodable(element))
                    {
                        return false;
                    }
                }

                return true;
            default:
                return true;
        }
    }
}
