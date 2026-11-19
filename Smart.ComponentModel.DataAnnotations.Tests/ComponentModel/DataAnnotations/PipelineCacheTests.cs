namespace Smart.ComponentModel.DataAnnotations;

using SourceGenerateHelper.Testing;

public sealed class PipelineCacheTests
{
    private const string Source =
        """
        using Smart.ComponentModel.DataAnnotations;

        namespace Test;

        public sealed class Form
        {
            public bool Enabled { get; set; }

            [RequiredWhen(nameof(Enabled))]
            public string? Name { get; set; }
        }
        """;

    private const string UnrelatedSource =
        """
        namespace Other;

        internal sealed class Unrelated;
        """;

    private const string AddedSource =
        """
        using Smart.ComponentModel.DataAnnotations;

        namespace Test;

        public sealed class AddedForm
        {
            public bool Visible { get; set; }

            [RequiredWhen(nameof(Visible))]
            public string? Name { get; set; }
        }
        """;

    [Fact]
    public void UnrelatedEditKeepsOutputCached()
    {
        // Arrange & Act
        var result = GeneratorTestHelper.RunIncremental(Source, UnrelatedSource);

        // Assert
        Assert.Equal(result.FirstGeneratedText, result.SecondGeneratedText);
        Assert.NotEmpty(result.OutputReasons);
        Assert.DoesNotContain(result.OutputReasons, static x => x.IsChanged());
    }

    [Fact]
    public void UnrelatedEditOfTargetFileKeepsRegistrationUnchanged()
    {
        // Arrange & Act
        var result = GeneratorTestHelper.RunIncrementalEdit(Source, Source + "\ninternal sealed class Unrelated;");

        // Assert
        Assert.Equal(result.FirstGeneratedText, result.SecondGeneratedText);
        Assert.NotEmpty(result.StepReasons("Registration"));
        Assert.DoesNotContain(result.StepReasons("Registration"), static x => x.IsChanged());
    }

    [Fact]
    public void AddedTargetRebuildsRegistration()
    {
        // Arrange & Act
        var result = GeneratorTestHelper.RunIncremental(Source, AddedSource);

        // Assert
        Assert.DoesNotContain("typeof(global::Test.AddedForm)", result.FirstGeneratedText, StringComparison.Ordinal);
        Assert.Contains("typeof(global::Test.AddedForm)", result.SecondGeneratedText, StringComparison.Ordinal);
    }
}
