namespace Smart.ComponentModel.DataAnnotations.Generator;

using System;
using System.Collections.Immutable;

using Microsoft.CodeAnalysis;

using Smart.ComponentModel.DataAnnotations.Generator.Helpers;
using Smart.ComponentModel.DataAnnotations.Generator.Models;

using SourceGenerateHelper;

internal static class ValidationModelBuilder
{
    internal const string CompareToAttributeName = "Smart.ComponentModel.DataAnnotations.CompareToAttribute";
    internal const string CompareToWhenAttributeName = "Smart.ComponentModel.DataAnnotations.CompareToWhenAttribute";
    internal const string EqualToAttributeName = "Smart.ComponentModel.DataAnnotations.EqualToAttribute";
    internal const string RequiredWhenAttributeName = "Smart.ComponentModel.DataAnnotations.RequiredWhenAttribute";
    internal const string EmptyWhenAttributeName = "Smart.ComponentModel.DataAnnotations.EmptyWhenAttribute";
    internal const string AnyRequiredAttributeName = "Smart.ComponentModel.DataAnnotations.AnyRequiredAttribute";
    internal const string AnyRequiredWhenAttributeName = "Smart.ComponentModel.DataAnnotations.AnyRequiredWhenAttribute";
    internal const string DuplicateKeyAttributeName = "Smart.ComponentModel.DataAnnotations.DuplicateKeyAttribute";
    internal const string MaxCountAttributeName = "Smart.ComponentModel.DataAnnotations.MaxCountAttribute";
    private const string DisplayAttributeName = "System.ComponentModel.DataAnnotations.DisplayAttribute";
    private const string DisplayNameAttributeName = "System.ComponentModel.DisplayNameAttribute";
    private const string ComparableName = "System.IComparable";
    private const string GenericComparableName = "System.IComparable`1";

    internal static readonly (AttributeKind Kind, string AttributeName)[] Targets =
    [
        (AttributeKind.CompareTo, CompareToAttributeName),
        (AttributeKind.CompareToWhen, CompareToWhenAttributeName),
        (AttributeKind.EqualTo, EqualToAttributeName),
        (AttributeKind.RequiredWhen, RequiredWhenAttributeName),
        (AttributeKind.EmptyWhen, EmptyWhenAttributeName),
        (AttributeKind.AnyRequired, AnyRequiredAttributeName),
        (AttributeKind.AnyRequiredWhen, AnyRequiredWhenAttributeName),
        (AttributeKind.DuplicateKey, DuplicateKeyAttributeName),
        (AttributeKind.MaxCount, MaxCountAttributeName)
    ];

    private static readonly string[] KindNames = Enum.GetNames(typeof(AttributeKind));

    // ------------------------------------------------------------
    // Build
    // ------------------------------------------------------------

    internal static bool HasRegistration(AttributeKind kind) =>
        kind != AttributeKind.MaxCount;

    internal static string GetName(AttributeKind kind) =>
        KindNames[(int)kind];

    internal static RegistrationModel Build(GeneratorAttributeSyntaxContext context, AttributeKind kind, CancellationToken cancellation)
    {
        var builder = new RegistrationBuilder(false);
        foreach (var attribute in context.Attributes)
        {
            cancellation.ThrowIfCancellationRequested();

            Read(builder, context.TargetSymbol, kind, attribute);
        }

        return builder.ToModel();
    }

    internal static IReadOnlyList<(DiagnosticDescriptor Descriptor, object[] Arguments)> Validate(ISymbol target, AttributeKind kind, AttributeData attribute)
    {
        var builder = new RegistrationBuilder(true);
        Read(builder, target, kind, attribute);
        return builder.Diagnostics;
    }

    // ------------------------------------------------------------
    // Read
    // ------------------------------------------------------------

    private static void Read(RegistrationBuilder builder, ISymbol target, AttributeKind kind, AttributeData attribute)
    {
        if (kind is AttributeKind.AnyRequired or AttributeKind.AnyRequiredWhen)
        {
            ReadType(builder, target, kind, attribute);
        }
        else
        {
            ReadMember(builder, target, kind, attribute);
        }
    }

    private static void ReadMember(RegistrationBuilder builder, ISymbol target, AttributeKind kind, AttributeData attribute)
    {
        if (kind == AttributeKind.MaxCount)
        {
            if (ReadInt(attribute, 0) is < 0)
            {
                builder.Report(Diagnostics.InvalidArgument, kind, "count");
            }

            return;
        }

        if (ResolveMember(target) is not { } member)
        {
            builder.Report(Diagnostics.UnsupportedTarget, kind, target.Name);
            return;
        }

        var (type, property) = member;
        if (!IsSupportedContainer(type))
        {
            builder.Report(Diagnostics.UnsupportedType, kind, type.ToDisplayString());
            return;
        }

        switch (kind)
        {
            case AttributeKind.CompareTo:
                AddComparison(builder, kind, type, property, ReadString(attribute, 1));
                break;
            case AttributeKind.CompareToWhen:
                AddCondition(builder, kind, type, ReadString(attribute, 0));
                AddComparison(builder, kind, type, property, ReadString(attribute, 2));
                break;
            case AttributeKind.EqualTo:
                AddEquality(builder, kind, type, property, ReadString(attribute, 0));
                break;
            case AttributeKind.RequiredWhen:
            case AttributeKind.EmptyWhen:
                AddCondition(builder, kind, type, ReadString(attribute, 0));
                if (IsAlwaysPresent(property.Type))
                {
                    builder.Report(Diagnostics.ValueAlwaysPresent, kind, type.ToDisplayString(), property.Name);
                }

                break;
            case AttributeKind.DuplicateKey:
                AddDuplicate(builder, kind, type, property, ReadStrings(attribute, 0));
                break;
        }
    }

    private static void ReadType(RegistrationBuilder builder, ISymbol target, AttributeKind kind, AttributeData attribute)
    {
        if (target is not INamedTypeSymbol type)
        {
            builder.Report(Diagnostics.UnsupportedTarget, kind, target.Name);
            return;
        }

        if (!IsSupportedContainer(type))
        {
            builder.Report(Diagnostics.UnsupportedType, kind, type.ToDisplayString());
            return;
        }

        var index = 0;
        if (kind == AttributeKind.AnyRequiredWhen)
        {
            AddCondition(builder, kind, type, ReadString(attribute, 0));
            index = 1;
        }

        var properties = ReadStrings(attribute, index);
        if ((properties is null) || (properties.Count == 0))
        {
            builder.Report(Diagnostics.InvalidArgument, kind, "properties");
            return;
        }

        foreach (var name in properties)
        {
            var property = String.IsNullOrEmpty(name) ? null : FindProperty(type, name!);
            if (property is null)
            {
                builder.Report(Diagnostics.MemberNotFound, kind, type.ToDisplayString(), name ?? string.Empty);
                continue;
            }

            if (IsAlwaysPresent(property.Type))
            {
                builder.Report(Diagnostics.ValueAlwaysPresent, kind, type.ToDisplayString(), property.Name);
            }

            AddMember(builder, kind, type, property);
            AddDisplayName(builder, type, property);
        }
    }

    private static void AddCondition(RegistrationBuilder builder, AttributeKind kind, INamedTypeSymbol type, string? name)
    {
        if (String.IsNullOrEmpty(name))
        {
            builder.Report(Diagnostics.InvalidArgument, kind, "conditionProperty");
            return;
        }

        var property = FindProperty(type, name!);
        if (property is null)
        {
            builder.Report(Diagnostics.MemberNotFound, kind, type.ToDisplayString(), name!);
            return;
        }

        if (!IsBoolean(property.Type))
        {
            builder.Report(Diagnostics.InvalidConditionType, kind, type.ToDisplayString(), property.Name);
            return;
        }

        AddMember(builder, kind, type, property);
    }

    private static void AddComparison(RegistrationBuilder builder, AttributeKind kind, INamedTypeSymbol type, IPropertySymbol property, string? otherName)
    {
        if (FindOther(builder, kind, type, otherName) is not { } other)
        {
            return;
        }

        var comparedType = GetUnderlyingType(property.Type);
        if (!SymbolEqualityComparer.Default.Equals(comparedType, GetUnderlyingType(other.Type)) ||
            (comparedType.TypeKind == TypeKind.Interface) ||
            !IsComparable(comparedType) ||
            !IsAccessibleType(comparedType))
        {
            builder.Report(Diagnostics.NotComparable, kind, type.ToDisplayString(), property.Name, other.Name);
            return;
        }

        AddMember(builder, kind, type, other);
        AddDisplayName(builder, type, other);
        builder.Comparers.Add(new ComparerModel(GetTypeName(comparedType)));
    }

    private static void AddEquality(RegistrationBuilder builder, AttributeKind kind, INamedTypeSymbol type, IPropertySymbol property, string? otherName)
    {
        if (FindOther(builder, kind, type, otherName) is not { } other)
        {
            return;
        }

        if (!SymbolEqualityComparer.Default.Equals(GetUnderlyingType(property.Type), GetUnderlyingType(other.Type)))
        {
            builder.Report(Diagnostics.NotSameType, kind, type.ToDisplayString(), property.Name, other.Name);
            return;
        }

        AddMember(builder, kind, type, other);
        AddDisplayName(builder, type, other);
    }

    private static void AddDuplicate(RegistrationBuilder builder, AttributeKind kind, INamedTypeSymbol type, IPropertySymbol property, List<string?>? members)
    {
        if ((members is null) || (members.Count == 0) || members.Any(static x => String.IsNullOrEmpty(x)))
        {
            builder.Report(Diagnostics.InvalidArgument, kind, "members");
            return;
        }

        if (FindElementType(property.Type) is not { } elementType)
        {
            builder.Report(Diagnostics.NotCollection, kind, type.ToDisplayString(), property.Name);
            return;
        }

        var keyOwner = GetUnderlyingType(elementType);
        if ((keyOwner.TypeKind == TypeKind.Interface) || !IsAccessibleType(keyOwner))
        {
            builder.Report(Diagnostics.UnsupportedType, kind, keyOwner.ToDisplayString());
            return;
        }

        foreach (var name in members)
        {
            var key = FindProperty(keyOwner, name!);
            if (key is null)
            {
                builder.Report(Diagnostics.MemberNotFound, kind, keyOwner.ToDisplayString(), name!);
                return;
            }

            AddMember(builder, kind, keyOwner, key);
        }
    }

    private static IPropertySymbol? FindOther(RegistrationBuilder builder, AttributeKind kind, INamedTypeSymbol type, string? otherName)
    {
        if (String.IsNullOrEmpty(otherName))
        {
            builder.Report(Diagnostics.InvalidArgument, kind, "otherProperty");
            return null;
        }

        var other = FindProperty(type, otherName!);
        if (other is null)
        {
            builder.Report(Diagnostics.MemberNotFound, kind, type.ToDisplayString(), otherName!);
        }

        return other;
    }

    private static void AddMember(RegistrationBuilder builder, AttributeKind kind, ITypeSymbol type, IPropertySymbol property)
    {
        if (property.Type.IsRefLikeType || (property.Type.TypeKind is TypeKind.Pointer or TypeKind.FunctionPointer))
        {
            builder.Report(Diagnostics.UnsupportedType, kind, property.Type.ToDisplayString());
            return;
        }

        builder.Members.Add(new MemberModel(GetTypeName(type), property.Name));
    }

    private static void AddDisplayName(RegistrationBuilder builder, INamedTypeSymbol type, IPropertySymbol property)
    {
        if (!AddDisplayName(builder, type, property, property.GetAttributes()) &&
            (FindPrimaryParameter(property) is { } parameter))
        {
            AddDisplayName(builder, type, property, parameter.GetAttributes());
        }
    }

    private static bool AddDisplayName(RegistrationBuilder builder, INamedTypeSymbol type, IPropertySymbol property, ImmutableArray<AttributeData> attributes)
    {
        foreach (var attribute in attributes)
        {
            if (attribute.AttributeClass.HasFullyQualifiedMetadataName(DisplayAttributeName))
            {
                if (attribute.TryGetNamedArgument<string>("Name", out var name) && !String.IsNullOrEmpty(name))
                {
                    var resourceType = attribute.TryGetNamedArgument("ResourceType", out var resource) && (resource.Value is ITypeSymbol symbol)
                        ? GetTypeName(symbol)
                        : null;
                    builder.DisplayNames.Add(new DisplayNameModel(GetTypeName(type), property.Name, name, resourceType));
                    return true;
                }
            }
            else if (attribute.AttributeClass.HasFullyQualifiedMetadataName(DisplayNameAttributeName))
            {
                if (attribute.TryGetConstructorArgument<string>(0, out var name) && !String.IsNullOrEmpty(name))
                {
                    builder.DisplayNames.Add(new DisplayNameModel(GetTypeName(type), property.Name, name, null));
                    return true;
                }
            }
        }

        return false;
    }

    // ------------------------------------------------------------
    // Symbol
    // ------------------------------------------------------------

    private static (INamedTypeSymbol Type, IPropertySymbol Property)? ResolveMember(ISymbol target)
    {
        switch (target)
        {
            case IPropertySymbol property:
                return (property.ContainingType, property);
            case IParameterSymbol { ContainingSymbol: IMethodSymbol { MethodKind: MethodKind.Constructor } constructor } parameter
                when constructor.ContainingType.IsRecord &&
                     constructor.ContainingType.TryGetRecordPrimaryConstructor(out var primary) &&
                     SymbolEqualityComparer.Default.Equals(primary, constructor):
                foreach (var member in constructor.ContainingType.GetMembers(parameter.Name))
                {
                    if (member is IPropertySymbol property)
                    {
                        return (constructor.ContainingType, property);
                    }
                }

                return null;
            default:
                return null;
        }
    }

    private static IParameterSymbol? FindPrimaryParameter(IPropertySymbol property)
    {
        if (!property.ContainingType.IsRecord || !property.ContainingType.TryGetRecordPrimaryConstructor(out var constructor))
        {
            return null;
        }

        foreach (var parameter in constructor.Parameters)
        {
            if (parameter.Name == property.Name)
            {
                return parameter;
            }
        }

        return null;
    }

    private static IPropertySymbol? FindProperty(ITypeSymbol type, string name)
    {
        for (var current = type; current is not null; current = current.BaseType)
        {
            foreach (var member in current.GetMembers(name))
            {
                if ((member is IPropertySymbol { IsStatic: false, IsIndexer: false, GetMethod: { } getter } property) &&
                    IsAccessible(getter.DeclaredAccessibility) &&
                    !IsObsoleteError(property))
                {
                    return property;
                }
            }
        }

        return null;
    }

    private static ITypeSymbol? FindElementType(ITypeSymbol type)
    {
        if (type.SpecialType == SpecialType.System_String)
        {
            return null;
        }

        if (type is IArrayTypeSymbol array)
        {
            return array.ElementType;
        }

        if ((type is INamedTypeSymbol named) && (named.ConstructedFrom.SpecialType == SpecialType.System_Collections_Generic_IEnumerable_T))
        {
            return named.TypeArguments[0];
        }

        foreach (var face in type.AllInterfaces)
        {
            if (face.ConstructedFrom.SpecialType == SpecialType.System_Collections_Generic_IEnumerable_T)
            {
                return face.TypeArguments[0];
            }
        }

        return null;
    }

    private static bool IsSupportedContainer(INamedTypeSymbol type)
    {
        if (type.TypeKind == TypeKind.Interface)
        {
            return false;
        }

        for (var current = type; current is not null; current = current.ContainingType)
        {
            if (!IsAccessible(current.DeclaredAccessibility) || (current.Arity > 0) || current.IsFileLocal || IsObsoleteError(current))
            {
                return false;
            }
        }

        return true;
    }

    private static bool IsAccessibleType(ITypeSymbol type)
    {
        switch (type)
        {
            case IArrayTypeSymbol array:
                return IsAccessibleType(array.ElementType);
            case INamedTypeSymbol named:
                for (var current = named; current is not null; current = current.ContainingType)
                {
                    if ((current.TypeKind == TypeKind.Error) || current.IsFileLocal || !IsAccessible(current.DeclaredAccessibility) || IsObsoleteError(current))
                    {
                        return false;
                    }

                    foreach (var argument in current.TypeArguments)
                    {
                        if (!IsAccessibleType(argument))
                        {
                            return false;
                        }
                    }
                }

                return true;
            default:
                return false;
        }
    }

    private static bool IsAccessible(Accessibility accessibility) =>
        accessibility is Accessibility.Public or Accessibility.Internal or Accessibility.ProtectedOrInternal;

    private static bool IsObsoleteError(ISymbol symbol) =>
        symbol.IsObsolete(out var isError) && isError;

    private static bool IsBoolean(ITypeSymbol type) =>
        GetUnderlyingType(type).SpecialType == SpecialType.System_Boolean;

    private static bool IsComparable(ITypeSymbol type)
    {
        foreach (var face in type.AllInterfaces)
        {
            if (face.HasFullyQualifiedMetadataName(ComparableName))
            {
                return true;
            }

            if (face.OriginalDefinition.HasFullyQualifiedMetadataName(GenericComparableName) &&
                SymbolEqualityComparer.Default.Equals(face.TypeArguments[0], type))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsAlwaysPresent(ITypeSymbol type) =>
        type.IsValueType && !IsNullableValue(type);

    private static bool IsNullableValue(ITypeSymbol type) =>
        type is INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Nullable_T };

    private static ITypeSymbol GetUnderlyingType(ITypeSymbol type) =>
        type is INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Nullable_T } named ? named.TypeArguments[0] : type;

    private static string GetTypeName(ITypeSymbol type) =>
        type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

    // ------------------------------------------------------------
    // Attribute
    // ------------------------------------------------------------

    private static string? ReadString(AttributeData attribute, int index) =>
        (index < attribute.ConstructorArguments.Length) && (attribute.ConstructorArguments[index].Kind != TypedConstantKind.Array)
            ? attribute.ConstructorArguments[index].Value as string
            : null;

    private static int? ReadInt(AttributeData attribute, int index) =>
        (index < attribute.ConstructorArguments.Length) && (attribute.ConstructorArguments[index].Value is int value)
            ? value
            : null;

    private static List<string?>? ReadStrings(AttributeData attribute, int index)
    {
        if ((index >= attribute.ConstructorArguments.Length) || (attribute.ConstructorArguments[index].Kind != TypedConstantKind.Array))
        {
            return null;
        }

        return attribute.ConstructorArguments[index].Values.Select(static x => x.Value as string).ToList();
    }

    // ------------------------------------------------------------
    // Builder
    // ------------------------------------------------------------

    private sealed class RegistrationBuilder
    {
        private readonly bool collectDiagnostics;

        private List<(DiagnosticDescriptor Descriptor, object[] Arguments)>? diagnostics;

        private bool hasErrors;

        public List<MemberModel> Members { get; } = [];

        public List<ComparerModel> Comparers { get; } = [];

        public List<DisplayNameModel> DisplayNames { get; } = [];

        public IReadOnlyList<(DiagnosticDescriptor Descriptor, object[] Arguments)> Diagnostics => diagnostics ?? [];

        public RegistrationBuilder(bool collectDiagnostics)
        {
            this.collectDiagnostics = collectDiagnostics;
        }

        public void Report(DiagnosticDescriptor descriptor, AttributeKind kind, params string[] arguments)
        {
            if (descriptor.DefaultSeverity == DiagnosticSeverity.Error)
            {
                hasErrors = true;
            }

            if (collectDiagnostics)
            {
                (diagnostics ??= []).Add((descriptor, [GetName(kind), .. arguments]));
            }
        }

        public RegistrationModel ToModel() =>
            hasErrors
                ? RegistrationModel.Empty
                : new RegistrationModel(
                    new EquatableArray<MemberModel>(Members),
                    new EquatableArray<ComparerModel>(Comparers),
                    new EquatableArray<DisplayNameModel>(DisplayNames));
    }
}
