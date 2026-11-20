namespace Smart.ComponentModel.DataAnnotations.AspNetCore.Adapters;

using Microsoft.AspNetCore.Mvc.ModelBinding;

internal static class ClientValueType
{
    public const string Number = "number";

    public const string Date = "date";

    public static string? Resolve(ModelMetadata metadata)
    {
        var type = metadata.UnderlyingOrModelType;
        if (type.IsEnum)
        {
            return null;
        }

        if ((type == typeof(DateOnly)) || (type == typeof(TimeOnly)) || (type == typeof(DateTime)) || (type == typeof(DateTimeOffset)))
        {
            return Date;
        }

        return Type.GetTypeCode(type) is >= TypeCode.SByte and <= TypeCode.Decimal ? Number : null;
    }
}
