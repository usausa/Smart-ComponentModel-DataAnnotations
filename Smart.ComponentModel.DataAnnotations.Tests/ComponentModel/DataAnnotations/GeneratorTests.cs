namespace Smart.ComponentModel.DataAnnotations;

public sealed class GeneratorTests
{
    private const string Source =
        """
        using System;
        using System.Collections.Generic;
        using System.ComponentModel.DataAnnotations;

        using Smart.ComponentModel.DataAnnotations;

        namespace Test;

        [AnyRequired(nameof(Phone), nameof(Mail))]
        public sealed class Form
        {
            public bool UsePeriod { get; set; }

            public string? Phone { get; set; }

            public string? Mail { get; set; }

            [Display(Name = "Start")]
            public DateOnly? From { get; set; }

            [RequiredWhen(nameof(UsePeriod))]
            [CompareTo(CompareToOperation.GreaterThanOrEqual, nameof(From))]
            public DateOnly? To { get; set; }

            [DuplicateKey(nameof(Entry.Code))]
            public List<Entry>? Entries { get; set; }
        }

        public sealed class Entry
        {
            public string? Code { get; set; }

            public int Branch { get; set; }
        }
        """;

    // ------------------------------------------------------------
    // Basic
    // ------------------------------------------------------------

    [Fact]
    public void RegistrationsAreGenerated()
    {
        // Arrange & Act
        var generated = GeneratorTestHelper.GetGeneratedSource(Source);

        // Assert
        Assert.Contains("[global::System.Runtime.CompilerServices.ModuleInitializer]", generated, StringComparison.Ordinal);
        Assert.Contains("RegisterCondition(typeof(global::Test.Form), \"UsePeriod\"", generated, StringComparison.Ordinal);
        Assert.Contains("RegisterPresence(typeof(global::Test.Form), \"Phone\"", generated, StringComparison.Ordinal);
        Assert.Contains("RegisterPresence(typeof(global::Test.Form), \"Mail\"", generated, StringComparison.Ordinal);
        Assert.Contains("RegisterComparison(typeof(global::Test.Form), \"To\", \"From\"", generated, StringComparison.Ordinal);
        Assert.Contains("Comparer<global::System.DateOnly>.Default.Compare(value, other)", generated, StringComparison.Ordinal);
        Assert.Contains("RegisterDuplicate(typeof(global::Test.Form), \"Entries\", \"Code\"", generated, StringComparison.Ordinal);
        Assert.Contains("RegisterDisplayName(typeof(global::Test.Form), \"From\", static () => \"Start\")", generated, StringComparison.Ordinal);
    }

    [Fact]
    public void GeneratedSourceCompiles()
    {
        // Arrange & Act
        var errors = GeneratorTestHelper.GetCompilationErrors(Source);

        // Assert
        Assert.Empty(errors);
    }

    [Fact]
    public void SourceWithoutAttributeGeneratesNothing()
    {
        // Arrange
        const string source =
            """
            namespace Test;

            public sealed class Form
            {
                public string? Name { get; set; }
            }
            """;

        // Act
        var sources = GeneratorTestHelper.GetGeneratedSources(source);

        // Assert
        Assert.Empty(sources);
    }

    // ------------------------------------------------------------
    // Shape
    // ------------------------------------------------------------

    [Fact]
    public void ValueTypeIsReadWithoutNullCheck()
    {
        // Arrange
        const string source =
            """
            using Smart.ComponentModel.DataAnnotations;

            namespace Test;

            public sealed class Form
            {
                public int Minimum { get; set; }

                [CompareTo(CompareToOperation.GreaterThanOrEqual, nameof(Minimum))]
                public int Maximum { get; set; }
            }
            """;

        // Act
        var generated = GeneratorTestHelper.GetGeneratedSource(source);

        // Assert
        Assert.Contains("var value = instance.Maximum;", generated, StringComparison.Ordinal);
        Assert.Contains("var other = instance.Minimum;", generated, StringComparison.Ordinal);
    }

    [Fact]
    public void CompositeKeyUsesTuple()
    {
        // Arrange
        const string source =
            """
            using System.Collections.Generic;

            using Smart.ComponentModel.DataAnnotations;

            namespace Test;

            public sealed class Form
            {
                [DuplicateKey(nameof(Entry.Code), nameof(Entry.Branch))]
                public List<Entry>? Entries { get; set; }
            }

            public sealed class Entry
            {
                public string? Code { get; set; }

                public int Branch { get; set; }
            }
            """;

        // Act
        var generated = GeneratorTestHelper.GetGeneratedSource(source);

        // Assert
        Assert.Contains("HashSet<(string, int)>", generated, StringComparison.Ordinal);
        Assert.Contains("RegisterDuplicate(typeof(global::Test.Form), \"Entries\", \"Code,Branch\"", generated, StringComparison.Ordinal);
    }

    [Fact]
    public void ResourceDisplayNameUsesDisplayAttribute()
    {
        // Arrange
        const string source =
            """
            using System.ComponentModel.DataAnnotations;

            using Smart.ComponentModel.DataAnnotations;

            namespace Test;

            public static class Resources
            {
                public static string Start => "Start";
            }

            public sealed class Form
            {
                [Display(Name = nameof(Resources.Start), ResourceType = typeof(Resources))]
                public int Minimum { get; set; }

                [CompareTo(CompareToOperation.GreaterThanOrEqual, nameof(Minimum))]
                public int Maximum { get; set; }
            }
            """;

        // Act
        var generated = GeneratorTestHelper.GetGeneratedSource(source);

        // Assert
        Assert.Contains("ResourceType = typeof(global::Test.Resources)", generated, StringComparison.Ordinal);
    }

    [Fact]
    public void RecordParameterAndNestedTypeAreSupported()
    {
        // Arrange
        const string source =
            """
            using Smart.ComponentModel.DataAnnotations;

            namespace Test;

            public static class Outer
            {
                public sealed record Inner(
                    bool Enabled,
                    [RequiredWhen(nameof(Inner.Enabled))] string? Name);
            }
            """;

        // Act
        var generated = GeneratorTestHelper.GetGeneratedSource(source);
        var errors = GeneratorTestHelper.GetCompilationErrors(source);

        // Assert
        Assert.Contains("RegisterCondition(typeof(global::Test.Outer.Inner), \"Enabled\"", generated, StringComparison.Ordinal);
        Assert.Empty(errors);
    }
}
