namespace Smart.ComponentModel.DataAnnotations;

using System.ComponentModel.DataAnnotations;

using Smart.ComponentModel.DataAnnotations.Internal;

internal static class ValidationHelper
{
    public static bool HasValue(object? value, bool allowEmptyStrings) =>
        (value is not null) && (allowEmptyStrings || (value is not string str) || !String.IsNullOrWhiteSpace(str));

    public static string[]? ToMemberNames(this ValidationContext validationContext) =>
        validationContext.MemberName is not null ? [validationContext.MemberName] : null;

    public static Func<string> ResolveDisplayName(Type type, string member) =>
        ValidationRegistry.FindDisplayName(type, member) ?? (() => member);
}
