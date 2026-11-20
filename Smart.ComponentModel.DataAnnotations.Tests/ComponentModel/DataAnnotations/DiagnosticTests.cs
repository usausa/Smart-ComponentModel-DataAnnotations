namespace Smart.ComponentModel.DataAnnotations;

public sealed class DiagnosticTests
{
    [Fact]
    public void ValidSourceEmitsNoDiagnostic()
    {
        // Arrange
        const string source =
            """
            using Smart.ComponentModel.DataAnnotations;

            namespace Test;

            [AnyRequired(nameof(Phone), nameof(Mail))]
            public sealed class Form
            {
                public bool Enabled { get; set; }

                public string? Phone { get; set; }

                public string? Mail { get; set; }

                [RequiredWhen(nameof(Enabled))]
                [EmptyWhen(nameof(Enabled), ConditionValue = false)]
                public int? Count { get; set; }
            }
            """;

        // Act
        var diagnostics = GeneratorTestHelper.GetDiagnosticIds(source);

        // Assert
        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Scd0001UnknownMemberEmitsDiagnostic()
    {
        // Arrange
        const string source =
            """
            using Smart.ComponentModel.DataAnnotations;

            namespace Test;

            public sealed class Form
            {
                [CompareTo(CompareToOperation.LessThan, "Unknown")]
                public int Value { get; set; }
            }
            """;

        // Act
        var diagnostics = GeneratorTestHelper.GetDiagnosticIds(source);

        // Assert
        Assert.Equal(["SCD0001"], diagnostics);
    }

    [Fact]
    public void Scd0001PrivateGetterEmitsDiagnostic()
    {
        // Arrange
        const string source =
            """
            using Smart.ComponentModel.DataAnnotations;

            namespace Test;

            [AnyRequired(nameof(Code))]
            public sealed class Form
            {
                public string? Code { private get; set; }
            }
            """;

        // Act
        var diagnostics = GeneratorTestHelper.GetDiagnosticIds(source);

        // Assert
        Assert.Equal(["SCD0001"], diagnostics);
    }

    [Fact]
    public void Scd0002NonBooleanConditionEmitsDiagnostic()
    {
        // Arrange
        const string source =
            """
            using Smart.ComponentModel.DataAnnotations;

            namespace Test;

            public sealed class Form
            {
                public int Mode { get; set; }

                [RequiredWhen(nameof(Mode))]
                public string? Name { get; set; }
            }
            """;

        // Act
        var diagnostics = GeneratorTestHelper.GetDiagnosticIds(source);

        // Assert
        Assert.Equal(["SCD0002"], diagnostics);
    }

    [Fact]
    public void Scd0003DifferentTypesEmitDiagnostic()
    {
        // Arrange
        const string source =
            """
            using Smart.ComponentModel.DataAnnotations;

            namespace Test;

            public sealed class Form
            {
                public long Minimum { get; set; }

                [CompareTo(CompareToOperation.GreaterThan, nameof(Minimum))]
                public int Maximum { get; set; }
            }
            """;

        // Act
        var diagnostics = GeneratorTestHelper.GetDiagnosticIds(source);

        // Assert
        Assert.Equal(["SCD0003"], diagnostics);
    }

    [Fact]
    public void Scd0003NotComparableTypeEmitsDiagnostic()
    {
        // Arrange
        const string source =
            """
            using Smart.ComponentModel.DataAnnotations;

            namespace Test;

            public sealed class Form
            {
                public object? Minimum { get; set; }

                [CompareTo(CompareToOperation.GreaterThan, nameof(Minimum))]
                public object? Maximum { get; set; }
            }
            """;

        // Act
        var diagnostics = GeneratorTestHelper.GetDiagnosticIds(source);

        // Assert
        Assert.Equal(["SCD0003"], diagnostics);
    }

    [Fact]
    public void Scd0004NotCollectionEmitsDiagnostic()
    {
        // Arrange
        const string source =
            """
            using Smart.ComponentModel.DataAnnotations;

            namespace Test;

            public sealed class Form
            {
                [DuplicateKey(nameof(string.Length))]
                public string? Name { get; set; }
            }
            """;

        // Act
        var diagnostics = GeneratorTestHelper.GetDiagnosticIds(source);

        // Assert
        Assert.Equal(["SCD0004"], diagnostics);
    }

    [Fact]
    public void Scd0005PrivateTypeEmitsDiagnostic()
    {
        // Arrange
        const string source =
            """
            using Smart.ComponentModel.DataAnnotations;

            namespace Test;

            public sealed class Outer
            {
                private sealed class Form
                {
                    public bool Enabled { get; set; }

                    [RequiredWhen(nameof(Enabled))]
                    public string? Name { get; set; }
                }
            }
            """;

        // Act
        var diagnostics = GeneratorTestHelper.GetDiagnosticIds(source);

        // Assert
        Assert.Equal(["SCD0005"], diagnostics);
    }

    [Fact]
    public void Scd0005GenericTypeEmitsDiagnostic()
    {
        // Arrange
        const string source =
            """
            using Smart.ComponentModel.DataAnnotations;

            namespace Test;

            public sealed class Form<T>
            {
                public bool Enabled { get; set; }

                [RequiredWhen(nameof(Enabled))]
                public T? Value { get; set; }
            }
            """;

        // Act
        var diagnostics = GeneratorTestHelper.GetDiagnosticIds(source);

        // Assert
        Assert.Equal(["SCD0005"], diagnostics);
    }

    [Fact]
    public void Scd0006NonNullableValueEmitsDiagnostic()
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
                public int Count { get; set; }
            }
            """;

        // Act
        var diagnostics = GeneratorTestHelper.GetDiagnosticIds(source);

        // Assert
        Assert.Equal(["SCD0006"], diagnostics);
    }

    [Fact]
    public void Scd0007EmptyPropertiesEmitDiagnostic()
    {
        // Arrange
        const string source =
            """
            using Smart.ComponentModel.DataAnnotations;

            namespace Test;

            [AnyRequired]
            public sealed class Form
            {
                public string? Name { get; set; }
            }
            """;

        // Act
        var diagnostics = GeneratorTestHelper.GetDiagnosticIds(source);

        // Assert
        Assert.Equal(["SCD0007"], diagnostics);
    }

    [Fact]
    public void Scd0007NegativeCountEmitsDiagnostic()
    {
        // Arrange
        const string source =
            """
            using System.Collections.Generic;

            using Smart.ComponentModel.DataAnnotations;

            namespace Test;

            public sealed class Form
            {
                [MaxCount(-1)]
                public List<string>? Items { get; set; }
            }
            """;

        // Act
        var diagnostics = GeneratorTestHelper.GetDiagnosticIds(source);

        // Assert
        Assert.Equal(["SCD0007"], diagnostics);
    }

    [Fact]
    public void Scd0008MethodParameterEmitsDiagnostic()
    {
        // Arrange
        const string source =
            """
            using Smart.ComponentModel.DataAnnotations;

            namespace Test;

            public static class Endpoint
            {
                public static void Handle(int minimum, [CompareTo(CompareToOperation.GreaterThan, "minimum")] int maximum)
                {
                }
            }
            """;

        // Act
        var diagnostics = GeneratorTestHelper.GetDiagnosticIds(source);

        // Assert
        Assert.Equal(["SCD0008"], diagnostics);
    }

    [Fact]
    public void Scd0001UnknownEqualToMemberEmitsDiagnostic()
    {
        // Arrange
        const string source =
            """
            using Smart.ComponentModel.DataAnnotations;

            namespace Test;

            public sealed class Form
            {
                [EqualTo("Unknown")]
                public string? Confirm { get; set; }
            }
            """;

        // Act
        var diagnostics = GeneratorTestHelper.GetDiagnosticIds(source);

        // Assert
        Assert.Equal(["SCD0001"], diagnostics);
    }

    [Fact]
    public void Scd0003InterfaceTypeEmitsDiagnostic()
    {
        // Arrange
        const string source =
            """
            using System;

            using Smart.ComponentModel.DataAnnotations;

            namespace Test;

            public interface ILevel : IComparable<ILevel>
            {
            }

            public sealed class Form
            {
                public ILevel? Minimum { get; set; }

                [CompareTo(CompareToOperation.GreaterThan, nameof(Minimum))]
                public ILevel? Maximum { get; set; }
            }
            """;

        // Act
        var diagnostics = GeneratorTestHelper.GetDiagnosticIds(source);

        // Assert
        Assert.Equal(["SCD0003"], diagnostics);
    }

    [Fact]
    public void Scd0005InterfaceContainerEmitsDiagnostic()
    {
        // Arrange
        const string source =
            """
            using Smart.ComponentModel.DataAnnotations;

            namespace Test;

            public interface IForm
            {
                bool Enabled { get; }

                [RequiredWhen(nameof(Enabled))]
                string? Name { get; }
            }
            """;

        // Act
        var diagnostics = GeneratorTestHelper.GetDiagnosticIds(source);

        // Assert
        Assert.Equal(["SCD0005"], diagnostics);
    }

    [Fact]
    public void Scd0005InterfaceElementTypeEmitsDiagnostic()
    {
        // Arrange
        const string source =
            """
            using System.Collections.Generic;

            using Smart.ComponentModel.DataAnnotations;

            namespace Test;

            public interface IEntry
            {
                string? Code { get; }
            }

            public sealed class Form
            {
                [DuplicateKey(nameof(IEntry.Code))]
                public List<IEntry>? Entries { get; set; }
            }
            """;

        // Act
        var diagnostics = GeneratorTestHelper.GetDiagnosticIds(source);

        // Assert
        Assert.Equal(["SCD0005"], diagnostics);
    }

    [Fact]
    public void Scd0005RefStructMemberEmitsDiagnostic()
    {
        // Arrange
        const string source =
            """
            using System;
            using System.Collections.Generic;

            using Smart.ComponentModel.DataAnnotations;

            namespace Test;

            public sealed class Entry
            {
                public Span<int> Code => default;
            }

            public sealed class Form
            {
                [DuplicateKey(nameof(Entry.Code))]
                public List<Entry>? Entries { get; set; }
            }
            """;

        // Act
        var diagnostics = GeneratorTestHelper.GetDiagnosticIds(source);

        // Assert
        Assert.Equal(["SCD0005"], diagnostics);
    }

    [Fact]
    public void Scd0009DifferentEqualToTypesEmitDiagnostic()
    {
        // Arrange
        const string source =
            """
            using Smart.ComponentModel.DataAnnotations;

            namespace Test;

            public sealed class Form
            {
                public long Code { get; set; }

                [EqualTo(nameof(Code))]
                public int? Confirm { get; set; }
            }
            """;

        // Act
        var diagnostics = GeneratorTestHelper.GetDiagnosticIds(source);

        // Assert
        Assert.Equal(["SCD0009"], diagnostics);
    }
}
