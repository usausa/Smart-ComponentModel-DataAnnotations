namespace Smart.ComponentModel.DataAnnotations.Generator;

using System;
using System.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

using Smart.ComponentModel.DataAnnotations.Generator.Models;

using SourceGenerateHelper;

[Generator]
public sealed class ValidationGenerator : IIncrementalGenerator
{
    private const string SourceName = "ValidationRegistration.g.cs";

    // ------------------------------------------------------------
    // Initialize
    // ------------------------------------------------------------

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValueProvider<EquatableArray<RegistrationModel>>? results = null;
        foreach (var (kind, attributeName) in ValidationModelBuilder.Targets)
        {
            if (!ValidationModelBuilder.HasRegistration(kind))
            {
                continue;
            }

            var provider = context.SyntaxProvider
                .ForAttributeWithMetadataName(
                    attributeName,
                    static (_, _) => true,
                    (syntaxContext, cancellation) => ValidationModelBuilder.Build(syntaxContext, kind, cancellation))
                .WithTrackingName(ValidationModelBuilder.GetName(kind))
                .Collect()
                .Select(static (values, _) => new EquatableArray<RegistrationModel>(values));

            results = results is { } left
                ? left.Combine(provider).Select(static (pair, _) => new EquatableArray<RegistrationModel>(pair.Left.Concat(pair.Right)))
                : provider;
        }

        var registrationProvider = results!.Value
            .Select(static (values, _) => Merge(values))
            .WithTrackingName("Registration");

        context.RegisterImplementationSourceOutput(
            registrationProvider,
            static (productionContext, registration) => Emit(productionContext, registration));
    }

    // ------------------------------------------------------------
    // Merge
    // ------------------------------------------------------------

    private static RegistrationModel Merge(EquatableArray<RegistrationModel> models) =>
        new(
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

    // ------------------------------------------------------------
    // Emit
    // ------------------------------------------------------------

    private static void Emit(SourceProductionContext context, RegistrationModel registration)
    {
        if (registration.IsEmpty)
        {
            return;
        }

        var source = ValidationSourceBuilder.Emit(registration);
        context.AddSource(SourceName, SourceText.From(source, Encoding.UTF8));
    }
}
