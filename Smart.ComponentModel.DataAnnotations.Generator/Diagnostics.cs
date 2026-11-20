namespace Smart.ComponentModel.DataAnnotations.Generator;

using Microsoft.CodeAnalysis;

using SourceGenerateHelper;

internal static class Diagnostics
{
    public static DiagnosticDescriptor MemberNotFound { get; } = new(
        id: "SCD0001",
        title: "Member not found",
        messageFormat: "[{0}] member is not found or has no getter accessible from the assembly. type=[{1}], member=[{2}]",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        customTags: DiagnosticTags.NotSuppressible);

    public static DiagnosticDescriptor InvalidConditionType { get; } = new(
        id: "SCD0002",
        title: "Invalid condition type",
        messageFormat: "[{0}] condition member must be bool or bool?. type=[{1}], member=[{2}]",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        customTags: DiagnosticTags.NotSuppressible);

    public static DiagnosticDescriptor NotComparable { get; } = new(
        id: "SCD0003",
        title: "Members are not comparable",
        messageFormat: "[{0}] members must be the same comparable type. type=[{1}], member=[{2}], other=[{3}]",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        customTags: DiagnosticTags.NotSuppressible);

    public static DiagnosticDescriptor NotCollection { get; } = new(
        id: "SCD0004",
        title: "Member is not a collection",
        messageFormat: "[{0}] member must be a collection other than string. type=[{1}], member=[{2}]",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        customTags: DiagnosticTags.NotSuppressible);

    public static DiagnosticDescriptor UnsupportedType { get; } = new(
        id: "SCD0005",
        title: "Unsupported type",
        messageFormat: "[{0}] type must be a non-generic class or struct accessible from the assembly. type=[{1}]",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        customTags: DiagnosticTags.NotSuppressible);

    public static DiagnosticDescriptor ValueAlwaysPresent { get; } = new(
        id: "SCD0006",
        title: "Value is always present",
        messageFormat: "[{0}] member is a non-nullable value type and always has a value. type=[{1}], member=[{2}]",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public static DiagnosticDescriptor InvalidArgument { get; } = new(
        id: "SCD0007",
        title: "Invalid attribute argument",
        messageFormat: "[{0}] argument is invalid. argument=[{1}]",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        customTags: DiagnosticTags.NotSuppressible);

    public static DiagnosticDescriptor UnsupportedTarget { get; } = new(
        id: "SCD0008",
        title: "Unsupported target",
        messageFormat: "[{0}] must be applied to a property or a record primary constructor parameter. target=[{1}]",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        customTags: DiagnosticTags.NotSuppressible);

    public static DiagnosticDescriptor NotSameType { get; } = new(
        id: "SCD0009",
        title: "Members are not the same type",
        messageFormat: "[{0}] members must be the same type. type=[{1}], member=[{2}], other=[{3}]",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        customTags: DiagnosticTags.NotSuppressible);
}
