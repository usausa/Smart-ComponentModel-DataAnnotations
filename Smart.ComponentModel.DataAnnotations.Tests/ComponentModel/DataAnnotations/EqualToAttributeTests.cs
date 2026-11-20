namespace Smart.ComponentModel.DataAnnotations;

public sealed class EqualToModel
{
    public string? Password { get; set; }

    [EqualTo(nameof(Password))]
    public string? ConfirmPassword { get; set; }
}

public sealed class EqualToDisplayModel
{
    [Display(Name = "Mail address")]
    public string? Mail { get; set; }

    [EqualTo(nameof(Mail))]
    public string? ConfirmMail { get; set; }
}

public sealed class EqualToValueModel
{
    public int Code { get; set; }

    [EqualTo(nameof(Code))]
    public int? ConfirmCode { get; set; }
}

public record EqualToRecord(
    string? Password,
    [EqualTo(nameof(EqualToRecord.Password))] string? ConfirmPassword);

public sealed class EqualToAttributeTests
{
    [Fact]
    public void SameValueIsValid()
    {
        // Arrange
        var model = new EqualToModel { Password = "secret", ConfirmPassword = "secret" };

        // Act
        var results = ValidationTestHelper.Validate(model);

        // Assert
        Assert.Empty(results);
    }

    [Fact]
    public void DifferentValueIsInvalid()
    {
        // Arrange
        var model = new EqualToModel { Password = "secret", ConfirmPassword = "Secret" };

        // Act
        var results = ValidationTestHelper.Validate(model);

        // Assert
        var result = Assert.Single(results);
        Assert.Equal("The field ConfirmPassword must match Password.", result.ErrorMessage);
        Assert.Equal(nameof(EqualToModel.ConfirmPassword), Assert.Single(result.MemberNames));
    }

    [Fact]
    public void NullsAreEqual()
    {
        // Arrange
        var model = new EqualToModel();

        // Act
        var results = ValidationTestHelper.Validate(model);

        // Assert
        Assert.Empty(results);
    }

    [Fact]
    public void NullAndValueAreNotEqual()
    {
        // Arrange
        var model = new EqualToModel { Password = "secret" };

        // Act
        var results = ValidationTestHelper.Validate(model);

        // Assert
        Assert.Single(results);
    }

    [Fact]
    public void ErrorUsesOtherDisplayName()
    {
        // Arrange
        var model = new EqualToDisplayModel { Mail = "a@example.com", ConfirmMail = "b@example.com" };

        // Act
        var results = ValidationTestHelper.Validate(model);

        // Assert
        var result = Assert.Single(results);
        Assert.Equal("The field ConfirmMail must match Mail address.", result.ErrorMessage);
    }

    [Theory]
    [InlineData(1, 1, true)]
    [InlineData(1, 2, false)]
    public void ValueTypesAreCompared(int code, int confirm, bool expected)
    {
        // Arrange
        var model = new EqualToValueModel { Code = code, ConfirmCode = confirm };

        // Act
        var results = ValidationTestHelper.Validate(model);

        // Assert
        Assert.Equal(expected, results.Count == 0);
    }

    [Fact]
    public void RecordParameterIsRegistered()
    {
        // Arrange
        var attribute = new EqualToAttribute(nameof(EqualToRecord.Password));
        var model = new EqualToRecord("secret", "other");

        // Act
        var result = ValidationTestHelper.ValidateProperty(attribute, model, nameof(EqualToRecord.ConfirmPassword), model.ConfirmPassword);

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public void ErrorMessageIsNotSetByDefault()
    {
        // Arrange & Act
        var attribute = new EqualToAttribute(nameof(EqualToModel.Password));

        // Assert
        Assert.Null(attribute.ErrorMessage);
    }

    [Fact]
    public void UnregisteredMemberThrows()
    {
        // Arrange
        var attribute = new EqualToAttribute("Unknown");
        var model = new EqualToModel { ConfirmPassword = "secret" };

        // Act
        var exception = Record.Exception(() => ValidationTestHelper.ValidateProperty(attribute, model, nameof(EqualToModel.ConfirmPassword), model.ConfirmPassword));

        // Assert
        Assert.IsType<InvalidOperationException>(exception);
    }
}
