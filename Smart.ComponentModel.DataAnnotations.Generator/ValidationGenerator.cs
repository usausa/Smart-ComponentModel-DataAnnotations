namespace Smart.ComponentModel.DataAnnotations.Generator;

using System;
using System.Collections.Immutable;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Smart.ComponentModel.DataAnnotations.Generator.Models;

using SourceGenerateHelper;

[Generator]
public sealed class ValidationGenerator : IIncrementalGenerator
{
    private const string AttributeNamespace = "Smart.ComponentModel.DataAnnotations.";
    private const string RegistryName = "global::Smart.ComponentModel.DataAnnotations.Internal.ValidationRegistry";
    private const string DisplayAttributeName = "System.ComponentModel.DataAnnotations.DisplayAttribute";
    private const string DisplayNameAttributeName = "System.ComponentModel.DisplayNameAttribute";
    private const string ComparableName = "System.IComparable";
    private const string GenericComparableName = "System.IComparable`1";
    private const string SourceName = "ValidationRegistration.g.cs";

    private static readonly AttributeKind[] Kinds =
    [
        AttributeKind.CompareTo,
        AttributeKind.CompareToWhen,
        AttributeKind.EqualTo,
        AttributeKind.RequiredWhen,
        AttributeKind.EmptyWhen,
        AttributeKind.AnyRequired,
        AttributeKind.AnyRequiredWhen,
        AttributeKind.DuplicateKey,
        AttributeKind.MaxCount
    ];

    private enum AttributeKind
    {
        CompareTo,
        CompareToWhen,
        EqualTo,
        RequiredWhen,
        EmptyWhen,
        AnyRequired,
        AnyRequiredWhen,
        DuplicateKey,
        MaxCount
    }

    // ------------------------------------------------------------
    // Initialize
    // ------------------------------------------------------------

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValueProvider<EquatableArray<Result<RegistrationModel>>>? results = null;
        IncrementalValueProvider<ImmutableArray<SyntaxTree>>? trees = null;
        foreach (var kind in Kinds)
        {
            var name = $"{AttributeNamespace}{kind}Attribute";
            var provider = context.SyntaxProvider
                .ForAttributeWithMetadataName(
                    name,
                    static (_, _) => true,
                    (syntaxContext, token) => GetRegistration(syntaxContext, kind, token))
                .WithTrackingName(kind.ToString())
                .Collect()
                .Select(static (values, _) => new EquatableArray<Result<RegistrationModel>>(values));
            var tree = context.ForAttributeWithMetadataNameSyntaxTrees(name, static (_, _) => true);

            results = results is { } left
                ? left.Combine(provider).Select(static (pair, _) => new EquatableArray<Result<RegistrationModel>>(pair.Left.Concat(pair.Right)))
                : provider;
            trees = trees is { } leftTrees
                ? leftTrees.Combine(tree).Select(static (pair, _) => pair.Left.AddRange(pair.Right))
                : tree;
        }

        var allResults = results!.Value;
        context.RegisterSourceOutput(
            allResults.Combine(trees!.Value),
            static (context, provider) => context.ReportDiagnostics(provider.Left.SelectError().Distinct(), provider.Right));

        var registrationProvider = allResults
            .Select(static (values, _) => Merge(values))
            .WithTrackingName("Registration");

        context.RegisterImplementationSourceOutput(
            registrationProvider,
            static (context, registration) => Execute(context, registration));
    }

    // ------------------------------------------------------------
    // Parser
    // ------------------------------------------------------------

    private static Result<RegistrationModel> GetRegistration(GeneratorAttributeSyntaxContext context, AttributeKind kind, CancellationToken token)
    {
        var builder = new RegistrationBuilder();
        foreach (var attribute in context.Attributes)
        {
            token.ThrowIfCancellationRequested();

            var location = GetLocation(attribute, context.TargetNode);
            if (kind is AttributeKind.AnyRequired or AttributeKind.AnyRequiredWhen)
            {
                ProcessType(builder, context, kind, attribute, location);
            }
            else
            {
                ProcessMember(builder, context, kind, attribute, location);
            }
        }

        return builder.ToResult();
    }

    private static void ProcessMember(RegistrationBuilder builder, GeneratorAttributeSyntaxContext context, AttributeKind kind, AttributeData attribute, Location location)
    {
        var attributeName = kind.ToString();
        if (kind == AttributeKind.MaxCount)
        {
            if (ReadInt(attribute, 0) is < 0)
            {
                builder.Diagnostics.Add(new DiagnosticInfo(Diagnostics.InvalidArgument, location, attributeName, "count"));
            }

            return;
        }

        if (ResolveMember(context) is not { } target)
        {
            builder.Diagnostics.Add(new DiagnosticInfo(Diagnostics.UnsupportedTarget, location, attributeName, context.TargetSymbol.Name));
            return;
        }

        var (type, property) = target;
        if (!IsSupportedContainer(type))
        {
            builder.Diagnostics.Add(new DiagnosticInfo(Diagnostics.UnsupportedType, location, attributeName, type.ToDisplayString()));
            return;
        }

        switch (kind)
        {
            case AttributeKind.CompareTo:
                AddComparison(builder, location, attributeName, type, property, ReadString(attribute, 1));
                break;
            case AttributeKind.CompareToWhen:
                AddCondition(builder, location, attributeName, type, ReadString(attribute, 0));
                AddComparison(builder, location, attributeName, type, property, ReadString(attribute, 2));
                break;
            case AttributeKind.EqualTo:
                AddEquality(builder, location, attributeName, type, property, ReadString(attribute, 0));
                break;
            case AttributeKind.RequiredWhen:
            case AttributeKind.EmptyWhen:
                AddCondition(builder, location, attributeName, type, ReadString(attribute, 0));
                if (IsAlwaysPresent(property.Type))
                {
                    builder.Diagnostics.Add(new DiagnosticInfo(Diagnostics.ValueAlwaysPresent, location, attributeName, type.ToDisplayString(), property.Name));
                }

                break;
            case AttributeKind.DuplicateKey:
                AddDuplicate(builder, location, attributeName, type, property, ReadStrings(attribute, 0));
                break;
        }
    }

    private static void ProcessType(RegistrationBuilder builder, GeneratorAttributeSyntaxContext context, AttributeKind kind, AttributeData attribute, Location location)
    {
        var attributeName = kind.ToString();
        if (context.TargetSymbol is not INamedTypeSymbol type)
        {
            builder.Diagnostics.Add(new DiagnosticInfo(Diagnostics.UnsupportedTarget, location, attributeName, context.TargetSymbol.Name));
            return;
        }

        if (!IsSupportedContainer(type))
        {
            builder.Diagnostics.Add(new DiagnosticInfo(Diagnostics.UnsupportedType, location, attributeName, type.ToDisplayString()));
            return;
        }

        var index = 0;
        if (kind == AttributeKind.AnyRequiredWhen)
        {
            AddCondition(builder, location, attributeName, type, ReadString(attribute, 0));
            index = 1;
        }

        var properties = ReadStrings(attribute, index);
        if ((properties is null) || (properties.Count == 0))
        {
            builder.Diagnostics.Add(new DiagnosticInfo(Diagnostics.InvalidArgument, location, attributeName, "properties"));
            return;
        }

        foreach (var name in properties)
        {
            var property = String.IsNullOrEmpty(name) ? null : FindProperty(type, name!);
            if (property is null)
            {
                builder.Diagnostics.Add(new DiagnosticInfo(Diagnostics.MemberNotFound, location, attributeName, type.ToDisplayString(), name ?? string.Empty));
                continue;
            }

            if (IsAlwaysPresent(property.Type))
            {
                builder.Diagnostics.Add(new DiagnosticInfo(Diagnostics.ValueAlwaysPresent, location, attributeName, type.ToDisplayString(), property.Name));
            }

            AddMember(builder, location, attributeName, type, property);
            AddDisplayName(builder, type, property);
        }
    }

    private static void AddCondition(RegistrationBuilder builder, Location location, string attributeName, INamedTypeSymbol type, string? name)
    {
        if (String.IsNullOrEmpty(name))
        {
            builder.Diagnostics.Add(new DiagnosticInfo(Diagnostics.InvalidArgument, location, attributeName, "conditionProperty"));
            return;
        }

        var property = FindProperty(type, name!);
        if (property is null)
        {
            builder.Diagnostics.Add(new DiagnosticInfo(Diagnostics.MemberNotFound, location, attributeName, type.ToDisplayString(), name!));
            return;
        }

        if (!IsBoolean(property.Type))
        {
            builder.Diagnostics.Add(new DiagnosticInfo(Diagnostics.InvalidConditionType, location, attributeName, type.ToDisplayString(), property.Name));
            return;
        }

        AddMember(builder, location, attributeName, type, property);
    }

    private static void AddComparison(RegistrationBuilder builder, Location location, string attributeName, INamedTypeSymbol type, IPropertySymbol property, string? otherName)
    {
        if (FindOther(builder, location, attributeName, type, otherName) is not { } other)
        {
            return;
        }

        var comparedType = GetUnderlyingType(property.Type);
        if (!SymbolEqualityComparer.Default.Equals(comparedType, GetUnderlyingType(other.Type)) ||
            (comparedType.TypeKind == TypeKind.Interface) ||
            !IsComparable(comparedType) ||
            !IsAccessibleType(comparedType))
        {
            builder.Diagnostics.Add(new DiagnosticInfo(Diagnostics.NotComparable, location, attributeName, type.ToDisplayString(), property.Name, other.Name));
            return;
        }

        AddMember(builder, location, attributeName, type, other);
        AddDisplayName(builder, type, other);
        builder.Comparers.Add(new ComparerModel(GetTypeName(comparedType)));
    }

    private static void AddEquality(RegistrationBuilder builder, Location location, string attributeName, INamedTypeSymbol type, IPropertySymbol property, string? otherName)
    {
        if (FindOther(builder, location, attributeName, type, otherName) is not { } other)
        {
            return;
        }

        if (!SymbolEqualityComparer.Default.Equals(GetUnderlyingType(property.Type), GetUnderlyingType(other.Type)))
        {
            builder.Diagnostics.Add(new DiagnosticInfo(Diagnostics.NotSameType, location, attributeName, type.ToDisplayString(), property.Name, other.Name));
            return;
        }

        AddMember(builder, location, attributeName, type, other);
        AddDisplayName(builder, type, other);
    }

    private static void AddDuplicate(RegistrationBuilder builder, Location location, string attributeName, INamedTypeSymbol type, IPropertySymbol property, List<string?>? members)
    {
        if ((members is null) || (members.Count == 0) || members.Any(static x => String.IsNullOrEmpty(x)))
        {
            builder.Diagnostics.Add(new DiagnosticInfo(Diagnostics.InvalidArgument, location, attributeName, "members"));
            return;
        }

        if (FindElementType(property.Type) is not { } elementType)
        {
            builder.Diagnostics.Add(new DiagnosticInfo(Diagnostics.NotCollection, location, attributeName, type.ToDisplayString(), property.Name));
            return;
        }

        var keyOwner = GetUnderlyingType(elementType);
        if ((keyOwner.TypeKind == TypeKind.Interface) || !IsAccessibleType(keyOwner))
        {
            builder.Diagnostics.Add(new DiagnosticInfo(Diagnostics.UnsupportedType, location, attributeName, keyOwner.ToDisplayString()));
            return;
        }

        foreach (var name in members)
        {
            var key = FindProperty(keyOwner, name!);
            if (key is null)
            {
                builder.Diagnostics.Add(new DiagnosticInfo(Diagnostics.MemberNotFound, location, attributeName, keyOwner.ToDisplayString(), name!));
                return;
            }

            AddMember(builder, location, attributeName, keyOwner, key);
        }
    }

    private static IPropertySymbol? FindOther(RegistrationBuilder builder, Location location, string attributeName, INamedTypeSymbol type, string? otherName)
    {
        if (String.IsNullOrEmpty(otherName))
        {
            builder.Diagnostics.Add(new DiagnosticInfo(Diagnostics.InvalidArgument, location, attributeName, "otherProperty"));
            return null;
        }

        var other = FindProperty(type, otherName!);
        if (other is null)
        {
            builder.Diagnostics.Add(new DiagnosticInfo(Diagnostics.MemberNotFound, location, attributeName, type.ToDisplayString(), otherName!));
        }

        return other;
    }

    private static void AddMember(RegistrationBuilder builder, Location location, string attributeName, ITypeSymbol type, IPropertySymbol property)
    {
        if (property.Type.IsRefLikeType || (property.Type.TypeKind is TypeKind.Pointer or TypeKind.FunctionPointer))
        {
            builder.Diagnostics.Add(new DiagnosticInfo(Diagnostics.UnsupportedType, location, attributeName, property.Type.ToDisplayString()));
            return;
        }

        builder.Members.Add(new MemberModel(GetTypeName(type), property.Name));
    }

    private static void AddDisplayName(RegistrationBuilder builder, INamedTypeSymbol type, IPropertySymbol property)
    {
        foreach (var attribute in GetDisplayAttributes(property))
        {
            var attributeType = attribute.AttributeClass?.ToDisplayString();
            if (attributeType == DisplayAttributeName)
            {
                if (attribute.TryGetNamedArgument<string>("Name", out var name) && !String.IsNullOrEmpty(name))
                {
                    var resourceType = attribute.TryGetNamedArgument("ResourceType", out var resource) && (resource.Value is ITypeSymbol symbol)
                        ? GetTypeName(symbol)
                        : null;
                    builder.DisplayNames.Add(new DisplayNameModel(GetTypeName(type), property.Name, name, resourceType));
                    return;
                }
            }
            else if (attributeType == DisplayNameAttributeName)
            {
                if (attribute.TryGetConstructorArgument<string>(0, out var name) && !String.IsNullOrEmpty(name))
                {
                    builder.DisplayNames.Add(new DisplayNameModel(GetTypeName(type), property.Name, name, null));
                    return;
                }
            }
        }
    }

    private static IEnumerable<AttributeData> GetDisplayAttributes(IPropertySymbol property)
    {
        foreach (var attribute in property.GetAttributes())
        {
            yield return attribute;
        }

        if (FindPrimaryParameter(property) is { } parameter)
        {
            foreach (var attribute in parameter.GetAttributes())
            {
                yield return attribute;
            }
        }
    }

    // ------------------------------------------------------------
    // Symbol
    // ------------------------------------------------------------

    private static (INamedTypeSymbol Type, IPropertySymbol Property)? ResolveMember(GeneratorAttributeSyntaxContext context)
    {
        switch (context.TargetSymbol)
        {
            case IPropertySymbol property:
                return (property.ContainingType, property);
            case IParameterSymbol { ContainingSymbol: IMethodSymbol { MethodKind: MethodKind.Constructor } constructor } parameter
                when context.TargetNode.Parent?.Parent is RecordDeclarationSyntax:
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
        if (!property.ContainingType.IsRecord)
        {
            return null;
        }

        foreach (var constructor in property.ContainingType.InstanceConstructors)
        {
            if (constructor.DeclaringSyntaxReferences.Length == 0)
            {
                continue;
            }

            if (constructor.DeclaringSyntaxReferences[0].GetSyntax() is not RecordDeclarationSyntax)
            {
                continue;
            }

            foreach (var parameter in constructor.Parameters)
            {
                if (parameter.Name == property.Name)
                {
                    return parameter;
                }
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

    private static Location GetLocation(AttributeData attribute, SyntaxNode target) =>
        attribute.ApplicationSyntaxReference?.GetSyntax().GetLocation() ?? target.GetLocation();

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
    // Generator
    // ------------------------------------------------------------

    private static RegistrationModel Merge(EquatableArray<Result<RegistrationModel>> results)
    {
        var models = results.SelectValue().ToList();
        return new RegistrationModel(
            new EquatableArray<MemberModel>(models.SelectMany(static x => x.Members)
                .Distinct()
                .OrderBy(static x => x.TypeName, StringComparer.Ordinal)
                .ThenBy(static x => x.Member, StringComparer.Ordinal)),
            new EquatableArray<ComparerModel>(models.SelectMany(static x => x.Comparers)
                .Distinct()
                .OrderBy(static x => x.TypeName, StringComparer.Ordinal)),
            new EquatableArray<DisplayNameModel>(models.SelectMany(static x => x.DisplayNames)
                .Distinct()
                .OrderBy(static x => x.TypeName, StringComparer.Ordinal)
                .ThenBy(static x => x.Member, StringComparer.Ordinal)));
    }

    private static void Execute(SourceProductionContext context, RegistrationModel registration)
    {
        if (registration.IsEmpty)
        {
            return;
        }

        var builder = new SourceBuilder();
        BuildSource(builder, registration);

        context.AddSource(SourceName, builder);
    }

    private static void BuildSource(SourceBuilder builder, RegistrationModel registration)
    {
        builder.AutoGenerated();
        builder.Indent().Append("#nullable disable").NewLine();
        builder.Disable("CS0612, CS0618");
        builder.NewLine();

        // type
        builder
            .Indent()
            .Append("file static class ValidationRegistration")
            .NewLine();
        builder.BeginScope();

        // method
        builder
            .Indent()
            .Append("[global::System.Runtime.CompilerServices.ModuleInitializer]")
            .NewLine();
        builder
            .Indent()
            .Append("public static void Initialize()")
            .NewLine();
        builder.BeginScope();

        foreach (var member in registration.Members)
        {
            BuildMember(builder, member);
        }

        foreach (var comparer in registration.Comparers)
        {
            BuildComparer(builder, comparer);
        }

        foreach (var displayName in registration.DisplayNames)
        {
            BuildDisplayName(builder, displayName);
        }

        builder.EndScope();
        builder.EndScope();
    }

    private static void BuildMember(SourceBuilder builder, MemberModel member)
    {
        builder
            .Indent()
            .Append(RegistryName)
            .Append(".RegisterMember(typeof(")
            .Append(member.TypeName)
            .Append("), ")
            .Append(CSharpLiteral.Format(member.Member))
            .Append(", static x => ((")
            .Append(member.TypeName)
            .Append(")x).")
            .Append(CSharpIdentifier.Escape(member.Member))
            .Append(");")
            .NewLine();
    }

    private static void BuildComparer(SourceBuilder builder, ComparerModel comparer)
    {
        builder
            .Indent()
            .Append(RegistryName)
            .Append(".RegisterComparer(typeof(")
            .Append(comparer.TypeName)
            .Append("), static (x, y) => global::System.Collections.Generic.Comparer<")
            .Append(comparer.TypeName)
            .Append(">.Default.Compare((")
            .Append(comparer.TypeName)
            .Append(")x, (")
            .Append(comparer.TypeName)
            .Append(")y));")
            .NewLine();
    }

    private static void BuildDisplayName(SourceBuilder builder, DisplayNameModel displayName)
    {
        builder
            .Indent()
            .Append(RegistryName)
            .Append(".RegisterDisplayName(typeof(")
            .Append(displayName.TypeName)
            .Append("), ")
            .Append(CSharpLiteral.Format(displayName.Member));

        if (displayName.ResourceTypeName is null)
        {
            builder
                .Append(", static () => ")
                .Append(CSharpLiteral.Format(displayName.Name))
                .Append(");")
                .NewLine();
        }
        else
        {
            builder
                .Append(", static () => new global::System.ComponentModel.DataAnnotations.DisplayAttribute { Name = ")
                .Append(CSharpLiteral.Format(displayName.Name))
                .Append(", ResourceType = typeof(")
                .Append(displayName.ResourceTypeName)
                .Append(") }.GetName() ?? ")
                .Append(CSharpLiteral.Format(displayName.Member))
                .Append(");")
                .NewLine();
        }
    }

    // ------------------------------------------------------------
    // Builder
    // ------------------------------------------------------------

    private sealed class RegistrationBuilder
    {
        public List<MemberModel> Members { get; } = [];

        public List<ComparerModel> Comparers { get; } = [];

        public List<DisplayNameModel> DisplayNames { get; } = [];

        public List<DiagnosticInfo> Diagnostics { get; } = [];

        public Result<RegistrationModel> ToResult()
        {
            var result = new Result<RegistrationModel>(
                new RegistrationModel(
                    new EquatableArray<MemberModel>(Members),
                    new EquatableArray<ComparerModel>(Comparers),
                    new EquatableArray<DisplayNameModel>(DisplayNames)),
                new EquatableArray<DiagnosticInfo>(Diagnostics));
            return result.HasErrors ? Results.Errors<RegistrationModel>(Diagnostics) : result;
        }
    }
}
