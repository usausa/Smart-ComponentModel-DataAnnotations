namespace Smart.ComponentModel.DataAnnotations;

using System.ComponentModel;

[AnyRequired(nameof(Phone), nameof(Mail))]
[AnyRequired(nameof(Code), nameof(Name))]
public sealed class AnyRequiredModel
{
    public string? Phone { get; set; }

    public string? Mail { get; set; }

    public string? Code { get; set; }

    public string? Name { get; set; }
}

[AnyRequired(nameof(Phone), nameof(Count))]
public sealed class AnyRequiredDisplayModel
{
    [Display(Name = "Phone number")]
    public string? Phone { get; set; }

    [DisplayName("Item count")]
    public int? Count { get; set; }
}

public sealed class AnyRequiredAttributeTests
{
    [Fact]
    public void OneValueInEachGroupIsValid()
    {
        // Arrange
        var model = new AnyRequiredModel { Mail = "user@example.com", Code = "A1" };

        // Act
        var results = ValidationTestHelper.Validate(model);

        // Assert
        Assert.Empty(results);
    }

    [Fact]
    public void EmptyGroupsReportEachGroup()
    {
        // Arrange
        var model = new AnyRequiredModel { Phone = " ", Code = "A1" };

        // Act
        var results = ValidationTestHelper.Validate(model);

        // Assert
        var result = Assert.Single(results);
        Assert.Equal("At least one of the fields Phone, Mail is required.", result.ErrorMessage);
        Assert.Collection(result.MemberNames, x => Assert.Equal(nameof(AnyRequiredModel.Phone), x), x => Assert.Equal(nameof(AnyRequiredModel.Mail), x));
    }

    [Fact]
    public void MultipleAttributesAreEvaluated()
    {
        // Arrange
        var model = new AnyRequiredModel();

        // Act
        var results = ValidationTestHelper.Validate(model);

        // Assert
        Assert.Equal(2, results.Count);
    }

    [Fact]
    public void ErrorUsesDisplayNames()
    {
        // Arrange
        var model = new AnyRequiredDisplayModel();

        // Act
        var results = ValidationTestHelper.Validate(model);

        // Assert
        var result = Assert.Single(results);
        Assert.Equal("At least one of the fields Phone number, Item count is required.", result.ErrorMessage);
    }

    [Fact]
    public void NullIsValid()
    {
        // Arrange
        var attribute = new AnyRequiredAttribute(nameof(AnyRequiredModel.Phone), nameof(AnyRequiredModel.Mail));
        var model = new AnyRequiredModel();

        // Act
        var result = attribute.GetValidationResult(null, new ValidationContext(model, nameof(AnyRequiredModel), null, null));

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void EmptyPropertiesThrow()
    {
        // Arrange
        var attribute = new AnyRequiredAttribute();
        var model = new AnyRequiredModel();

        // Act
        var exception = Record.Exception(() => attribute.GetValidationResult(model, new ValidationContext(model, nameof(AnyRequiredModel), null, null)));

        // Assert
        Assert.IsType<InvalidOperationException>(exception);
    }
}
