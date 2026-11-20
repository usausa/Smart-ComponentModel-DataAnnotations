namespace Smart.ComponentModel.DataAnnotations.Generator;

using System.Collections.Immutable;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ValidationAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
    [
        Diagnostics.MemberNotFound,
        Diagnostics.InvalidConditionType,
        Diagnostics.NotComparable,
        Diagnostics.NotCollection,
        Diagnostics.UnsupportedType,
        Diagnostics.ValueAlwaysPresent,
        Diagnostics.InvalidArgument,
        Diagnostics.UnsupportedTarget,
        Diagnostics.NotSameType
    ];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(static startContext =>
        {
            var attributeTypes = ResolveAttributeTypes(startContext.Compilation);
            if (attributeTypes.IsEmpty)
            {
                return;
            }

            startContext.RegisterSymbolAction(
                symbolContext => AnalyzeSymbol(symbolContext, attributeTypes),
                SymbolKind.NamedType,
                SymbolKind.Property,
                SymbolKind.Method);
        });
    }

    private static ImmutableArray<(INamedTypeSymbol Type, AttributeKind Kind)> ResolveAttributeTypes(Compilation compilation)
    {
        var builder = ImmutableArray.CreateBuilder<(INamedTypeSymbol Type, AttributeKind Kind)>();
        foreach (var (kind, attributeName) in ValidationModelBuilder.Targets)
        {
            if (compilation.GetTypeByMetadataName(attributeName) is { } type)
            {
                builder.Add((type, kind));
            }
        }

        return builder.ToImmutable();
    }

    private static void AnalyzeSymbol(SymbolAnalysisContext context, ImmutableArray<(INamedTypeSymbol Type, AttributeKind Kind)> attributeTypes)
    {
        if (context.Symbol is IMethodSymbol method)
        {
            foreach (var parameter in method.Parameters)
            {
                AnalyzeAttributes(context, parameter, attributeTypes);
            }
        }
        else
        {
            AnalyzeAttributes(context, context.Symbol, attributeTypes);
        }
    }

    private static void AnalyzeAttributes(SymbolAnalysisContext context, ISymbol target, ImmutableArray<(INamedTypeSymbol Type, AttributeKind Kind)> attributeTypes)
    {
        foreach (var attribute in target.GetAttributes())
        {
            if (FindKind(attributeTypes, attribute.AttributeClass) is not { } kind)
            {
                continue;
            }

            var diagnostics = ValidationModelBuilder.Validate(target, kind, attribute);
            if (diagnostics.Count == 0)
            {
                continue;
            }

            var location = attribute.ApplicationSyntaxReference?.GetSyntax(context.CancellationToken).GetLocation() ?? Location.None;
            foreach (var (descriptor, arguments) in diagnostics)
            {
                context.ReportDiagnostic(Diagnostic.Create(descriptor, location, arguments));
            }
        }
    }

    private static AttributeKind? FindKind(ImmutableArray<(INamedTypeSymbol Type, AttributeKind Kind)> attributeTypes, INamedTypeSymbol? attributeClass)
    {
        if (attributeClass is null)
        {
            return null;
        }

        foreach (var (type, kind) in attributeTypes)
        {
            if (SymbolEqualityComparer.Default.Equals(type, attributeClass))
            {
                return kind;
            }
        }

        return null;
    }
}
