namespace Smart.ComponentModel.DataAnnotations;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

using Smart.ComponentModel.DataAnnotations.Generator;

using SourceGenerateHelper.Testing;

internal static class GeneratorTestHelper
{
    private static GeneratorTestRunner Runner => GeneratorTestRunner
        .For<ValidationGenerator>()
        .WithReference(typeof(CompareToAttribute).Assembly)
        .WithDiagnosticPrefix("SCD")
        .VerifyCompiles();

    public static IReadOnlyList<Diagnostic> GetDiagnostics(string source)
    {
        var (_, compilation) = Runner.CreateDriver(source);
        var diagnostics = compilation
            .WithAnalyzers([new ValidationAnalyzer()])
            .GetAnalyzerDiagnosticsAsync()
            .GetAwaiter()
            .GetResult();
        return [.. diagnostics.Where(static x => x.Id.StartsWith("SCD", StringComparison.Ordinal)).OrderBy(static x => x.Location.SourceSpan.Start)];
    }

    public static IReadOnlyList<string> GetDiagnosticIds(string source) =>
        [.. GetDiagnostics(source).Select(static x => x.Id)];

    public static IReadOnlyList<Diagnostic> GetGeneratorDiagnostics(string source) =>
        Runner.VerifyCompiles(false).Run(source).GeneratorDiagnostics;

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
