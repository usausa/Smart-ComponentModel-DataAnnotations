namespace Smart.ComponentModel.DataAnnotations;

using Microsoft.CodeAnalysis;

using Smart.ComponentModel.DataAnnotations.Generator;

using SourceGenerateHelper.Testing;

internal static class GeneratorTestHelper
{
    private static GeneratorTestRunner Runner => GeneratorTestRunner
        .For<ValidationGenerator>()
        .WithReference(typeof(CompareToAttribute).Assembly)
        .WithDiagnosticPrefix("SCD")
        .VerifyCompiles();

    public static IReadOnlyList<string> GetDiagnosticIds(string source) =>
        [.. Runner.VerifyCompiles(false).GetDiagnostics(source).Select(static x => x.Id)];

    public static IReadOnlyList<Diagnostic> GetCompilationErrors(string source) =>
        Runner.VerifyCompiles(false).Run(source).CompilationErrors;

    public static string GetGeneratedSource(string source) => Runner.GetGeneratedSource(source);

    public static IReadOnlyDictionary<string, string> GetGeneratedSources(string source) =>
        Runner.VerifyCompiles(false).Run(source).GeneratedSources;

    public static IncrementalRunResult RunIncremental(string source, string addedSource) =>
        Runner.WithTracking().RunIncremental(source, addedSource);

    public static IncrementalRunResult RunIncrementalEdit(string source, string editedSource) =>
        Runner.WithTracking().RunIncrementalEdit(source, editedSource);
}
