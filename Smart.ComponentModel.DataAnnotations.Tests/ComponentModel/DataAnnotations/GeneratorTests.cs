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

            [EqualTo(nameof(Mail))]
            public string? ConfirmMail { get; set; }

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
        Assert.Contains("RegisterMember(typeof(global::Test.Form), \"UsePeriod\", static x => ((global::Test.Form)x).UsePeriod)", generated, StringComparison.Ordinal);
        Assert.Contains("RegisterMember(typeof(global::Test.Form), \"Phone\"", generated, StringComparison.Ordinal);
        Assert.Contains("RegisterMember(typeof(global::Test.Form), \"Mail\"", generated, StringComparison.Ordinal);
        Assert.Contains("RegisterMember(typeof(global::Test.Form), \"From\"", generated, StringComparison.Ordinal);
        Assert.Contains("RegisterMember(typeof(global::Test.Entry), \"Code\"", generated, StringComparison.Ordinal);
        Assert.Contains("RegisterComparer(typeof(global::System.DateOnly), static (x, y) => global::System.Collections.Generic.Comparer<global::System.DateOnly>.Default.Compare((global::System.DateOnly)x, (global::System.DateOnly)y))", generated, StringComparison.Ordinal);
        Assert.Contains("RegisterDisplayName(typeof(global::Test.Form), \"From\", static () => \"Start\")", generated, StringComparison.Ordinal);
    }

    [Fact]
    public void ValidatedMembersAreNotRead()
    {
        // Arrange & Act
        var generated = GeneratorTestHelper.GetGeneratedSource(Source);

        // Assert
        Assert.DoesNotContain("\"To\"", generated, StringComparison.Ordinal);
        Assert.DoesNotContain("\"ConfirmMail\"", generated, StringComparison.Ordinal);
        Assert.DoesNotContain("\"Entries\"", generated, StringComparison.Ordinal);
    }

    [Fact]
    public void SameMemberIsRegisteredOnce()
    {
        // Arrange & Act
        var generated = GeneratorTestHelper.GetGeneratedSource(Source);

        // Assert
        Assert.Single(generated.Split("RegisterMember(typeof(global::Test.Form), \"Mail\"").Skip(1));
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

    [Fact]
    public void InvalidUsageIsNotRegistered()
    {
        // Arrange
        const string source =
            """
            using Smart.ComponentModel.DataAnnotations;

            namespace Test;

            public sealed class Form
            {
                public bool Enabled { get; set; }

                [RequiredWhen(nameof(Enabled))]
                public string? Name { get; set; }

                [CompareTo(CompareToOperation.LessThan, "Unknown")]
                public int Value { get; set; }
            }
            """;

        // Act
        var generated = GeneratorTestHelper.GetGeneratedSource(source);
        var diagnostics = GeneratorTestHelper.GetGeneratorDiagnostics(source);

        // Assert
        Assert.Contains("RegisterMember(typeof(global::Test.Form), \"Enabled\"", generated, StringComparison.Ordinal);
        Assert.DoesNotContain("Unknown", generated, StringComparison.Ordinal);
        Assert.DoesNotContain("RegisterComparer", generated, StringComparison.Ordinal);
        Assert.Empty(diagnostics);
    }

    // ------------------------------------------------------------
    // Shape
    // ------------------------------------------------------------

    [Fact]
    public void ComparerIsGeneratedOncePerType()
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

                [CompareTo(CompareToOperation.LessThanOrEqual, nameof(Maximum))]
                public int? Value { get; set; }
            }
            """;

        // Act
        var generated = GeneratorTestHelper.GetGeneratedSource(source);

        // Assert
        Assert.Single(generated.Split("RegisterComparer(typeof(int)").Skip(1));
        Assert.Contains("Comparer<int>.Default.Compare((int)x, (int)y)", generated, StringComparison.Ordinal);
    }

    [Fact]
    public void KeyMembersAreRegisteredOnElementType()
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

                [DuplicateKey(nameof(Value.Code))]
                public Value?[]? Values { get; set; }
            }

            public sealed class Entry
            {
                public string? Code { get; set; }

                public int Branch { get; set; }
            }

            public struct Value
            {
                public int Code { get; set; }
            }
            """;

        // Act
        var generated = GeneratorTestHelper.GetGeneratedSource(source);
        var errors = GeneratorTestHelper.GetCompilationErrors(source);

        // Assert
        Assert.Contains("RegisterMember(typeof(global::Test.Entry), \"Code\"", generated, StringComparison.Ordinal);
        Assert.Contains("RegisterMember(typeof(global::Test.Entry), \"Branch\"", generated, StringComparison.Ordinal);
        Assert.Contains("RegisterMember(typeof(global::Test.Value), \"Code\", static x => ((global::Test.Value)x).Code)", generated, StringComparison.Ordinal);
        Assert.Empty(errors);
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
        Assert.Contains("RegisterMember(typeof(global::Test.Outer.Inner), \"Enabled\"", generated, StringComparison.Ordinal);
        Assert.Empty(errors);
    }
}
