namespace Smart.ComponentModel.DataAnnotations.Generator.Helpers;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

internal static class ValidationSymbolExtensions
{
    public static bool TryGetRecordPrimaryConstructor(this INamedTypeSymbol type, out IMethodSymbol constructor)
    {
        foreach (var candidate in type.InstanceConstructors)
        {
            if (candidate.Parameters.IsDefaultOrEmpty)
            {
                continue;
            }

            foreach (var reference in candidate.DeclaringSyntaxReferences)
            {
                if (reference.GetSyntax() is RecordDeclarationSyntax)
                {
                    constructor = candidate;
                    return true;
                }
            }
        }

        constructor = null!;
        return false;
    }
}
