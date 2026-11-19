namespace Smart.ComponentModel.DataAnnotations;

public sealed class CompareToModel
{
    public int Minimum { get; set; }

    [CompareTo(CompareToOperation.GreaterThanOrEqual, nameof(Minimum))]
    public int? Maximum { get; set; }
}

public sealed class CompareToNullableModel
{
    public DateOnly? From { get; set; }

    [CompareTo(CompareToOperation.GreaterThanOrEqual, nameof(From))]
    public DateOnly? To { get; set; }
}

public sealed class CompareToDisplayModel
{
    [Display(Name = "Lower limit")]
    public int Minimum { get; set; }

    [CompareTo(CompareToOperation.GreaterThanOrEqual, nameof(Minimum))]
    public int Maximum { get; set; }
}

public sealed class CompareToMessageModel
{
    [Display(Name = "Lower limit")]
    public int Minimum { get; set; }

    [CompareTo(CompareToOperation.GreaterThanOrEqual, nameof(Minimum), ErrorMessage = "{0} is less than {1}.")]
    public int Maximum { get; set; }
}

public sealed class CompareToResourceModel
{
    public int Minimum { get; set; }

    [CompareTo(CompareToOperation.GreaterThanOrEqual, nameof(Minimum), ErrorMessageResourceType = typeof(CompareToResources), ErrorMessageResourceName = nameof(CompareToResources.Message))]
    public int Maximum { get; set; }
}

public static class CompareToResources
{
    public static string Message => "{0} from resource {1}.";
}

public class CompareToBaseModel
{
    public string? Start { get; set; }

    [CompareTo(CompareToOperation.GreaterThan, nameof(Start))]
    public string? End { get; set; }
}

public sealed class CompareToDerivedModel : CompareToBaseModel
{
    public string? Note { get; set; }
}

public record CompareToRecord(
    int Minimum,
    [CompareTo(CompareToOperation.GreaterThanOrEqual, nameof(CompareToRecord.Minimum))] int Maximum);

internal struct CompareToStruct
{
    public long Minimum { get; set; }

    [CompareTo(CompareToOperation.LessThan, nameof(Minimum))]
    public long Maximum { get; set; }
}

public sealed class CompareToAttributeTests
{
    [Theory]
    [InlineData(CompareToOperation.LessThan, 1, 2, true)]
    [InlineData(CompareToOperation.LessThan, 2, 2, false)]
    [InlineData(CompareToOperation.LessThanOrEqual, 2, 2, true)]
    [InlineData(CompareToOperation.LessThanOrEqual, 3, 2, false)]
    [InlineData(CompareToOperation.GreaterThan, 3, 2, true)]
    [InlineData(CompareToOperation.GreaterThan, 2, 2, false)]
    [InlineData(CompareToOperation.GreaterThanOrEqual, 2, 2, true)]
    [InlineData(CompareToOperation.GreaterThanOrEqual, 1, 2, false)]
    public void IsValidComparesWithOtherProperty(CompareToOperation operation, int value, int other, bool expected)
    {
        // Arrange
        var attribute = new CompareToAttribute(operation, nameof(CompareToModel.Minimum));
        var model = new CompareToModel { Minimum = other, Maximum = value };

        // Act
        var result = ValidationTestHelper.ValidateProperty(attribute, model, nameof(CompareToModel.Maximum), value);

        // Assert
        Assert.Equal(expected, result is null);
    }

    [Fact]
    public void NullIsValid()
    {
        // Arrange
        var model = new CompareToModel { Minimum = 5, Maximum = null };

        // Act
        var results = ValidationTestHelper.Validate(model);

        // Assert
        Assert.Empty(results);
    }

    [Fact]
    public void NullOtherIsValid()
    {
        // Arrange
        var model = new CompareToNullableModel { From = null, To = new DateOnly(2026, 1, 1) };

        // Act
        var results = ValidationTestHelper.Validate(model);

        // Assert
        Assert.Empty(results);
    }

    [Fact]
    public void ErrorUsesOtherPropertyName()
    {
        // Arrange
        var model = new CompareToModel { Minimum = 5, Maximum = 3 };

        // Act
        var results = ValidationTestHelper.Validate(model);

        // Assert
        var result = Assert.Single(results);
        Assert.Equal("The field Maximum must be greater than or equal to Minimum.", result.ErrorMessage);
        Assert.Equal(nameof(CompareToModel.Maximum), Assert.Single(result.MemberNames));
    }

    [Fact]
    public void ErrorUsesOtherDisplayName()
    {
        // Arrange
        var model = new CompareToDisplayModel { Minimum = 5, Maximum = 3 };

        // Act
        var results = ValidationTestHelper.Validate(model);

        // Assert
        var result = Assert.Single(results);
        Assert.Equal("The field Maximum must be greater than or equal to Lower limit.", result.ErrorMessage);
    }

    [Fact]
    public void ErrorMessageIsNotSetByDefault()
    {
        // Arrange & Act
        var attribute = new CompareToAttribute(CompareToOperation.LessThan, nameof(CompareToModel.Minimum));

        // Assert
        Assert.Null(attribute.ErrorMessage);
    }

    [Fact]
    public void CustomErrorMessageUsesArguments()
    {
        // Arrange
        var model = new CompareToMessageModel { Minimum = 5, Maximum = 3 };

        // Act
        var results = ValidationTestHelper.Validate(model);

        // Assert
        var result = Assert.Single(results);
        Assert.Equal("Maximum is less than Lower limit.", result.ErrorMessage);
    }

    [Fact]
    public void ResourceErrorMessageUsesArguments()
    {
        // Arrange
        var model = new CompareToResourceModel { Minimum = 5, Maximum = 3 };

        // Act
        var results = ValidationTestHelper.Validate(model);

        // Assert
        var result = Assert.Single(results);
        Assert.Equal("Maximum from resource Minimum.", result.ErrorMessage);
    }

    [Fact]
    public void DerivedTypeUsesBaseRegistration()
    {
        // Arrange
        var model = new CompareToDerivedModel { Start = "b", End = "a" };

        // Act
        var results = ValidationTestHelper.Validate(model);

        // Assert
        var result = Assert.Single(results);
        Assert.Equal(nameof(CompareToBaseModel.End), Assert.Single(result.MemberNames));
    }

    [Fact]
    public void RecordParameterIsRegistered()
    {
        // Arrange
        var attribute = new CompareToAttribute(CompareToOperation.GreaterThanOrEqual, nameof(CompareToRecord.Minimum));
        var model = new CompareToRecord(5, 3);

        // Act
        var result = ValidationTestHelper.ValidateProperty(attribute, model, nameof(CompareToRecord.Maximum), model.Maximum);

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public void StructIsRegistered()
    {
        // Arrange
        var model = new CompareToStruct { Minimum = 1, Maximum = 2 };

        // Act
        var results = ValidationTestHelper.Validate(model);

        // Assert
        Assert.Single(results);
    }

    [Fact]
    public void UnregisteredMemberThrows()
    {
        // Arrange
        var attribute = new CompareToAttribute(CompareToOperation.GreaterThanOrEqual, "Unknown");
        var model = new CompareToModel { Minimum = 5, Maximum = 3 };

        // Act
        var exception = Record.Exception(() => ValidationTestHelper.ValidateProperty(attribute, model, nameof(CompareToModel.Maximum), model.Maximum));

        // Assert
        Assert.IsType<InvalidOperationException>(exception);
    }
}
