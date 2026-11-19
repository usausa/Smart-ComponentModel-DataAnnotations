namespace Smart.ComponentModel.DataAnnotations;

using System.ComponentModel.DataAnnotations;

using Smart.ComponentModel.DataAnnotations.Internal;

internal static class ValidationHelper
{
    public static bool HasValue(object? value, bool allowEmptyStrings) =>
        (value is not null) && (allowEmptyStrings || (value is not string str) || !String.IsNullOrWhiteSpace(str));

    public static string[]? ToMemberNames(this ValidationContext validationContext) =>
        validationContext.MemberName is not null ? [validationContext.MemberName] : null;

    public static string GetMemberName(this ValidationContext validationContext) =>
        validationContext.MemberName ?? throw new InvalidOperationException($"Member name is required. type=[{validationContext.ObjectType.FullName}]");

    public static string ResolveDisplayName(Type type, string member) =>
        ValidationRegistry.FindDisplayName(type, member)?.Invoke() ?? member;
}
