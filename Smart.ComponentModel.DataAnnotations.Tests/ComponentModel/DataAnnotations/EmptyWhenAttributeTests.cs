namespace Smart.ComponentModel.DataAnnotations;

public sealed class RequiredOrEmptyModel
{
    public bool? UseLimit { get; set; }

    [RequiredWhen(nameof(UseLimit))]
    [EmptyWhen(nameof(UseLimit), ConditionValue = false)]
    public int? Limit { get; set; }
}

public sealed class EmptyWhenMessageModel
{
    public bool UseLimit { get; set; }

    [EmptyWhen(nameof(UseLimit), ConditionValue = false, ErrorMessage = "{0} is not allowed.")]
    public int? Limit { get; set; }
}

public sealed class EmptyWhenStringModel
{
    public bool Clear { get; set; }

    [EmptyWhen(nameof(Clear))]
    public string? Name { get; set; }

    [EmptyWhen(nameof(Clear), AllowEmptyStrings = true)]
    public string? Note { get; set; }
}

public sealed class EmptyWhenAttributeTests
{
    [Theory]
    [InlineData(true, 10)]
    [InlineData(false, null)]
    [InlineData(null, 10)]
    [InlineData(null, null)]
    public void IsValidMatchesCondition(bool? useLimit, int? limit)
    {
        // Arrange
        var model = new RequiredOrEmptyModel { UseLimit = useLimit, Limit = limit };

        // Act
        var results = ValidationTestHelper.Validate(model);

        // Assert
        Assert.Empty(results);
    }

    [Fact]
    public void MetConditionRequiresValue()
    {
        // Arrange
        var model = new RequiredOrEmptyModel { UseLimit = true, Limit = null };

        // Act
        var results = ValidationTestHelper.Validate(model);

        // Assert
        var result = Assert.Single(results);
        Assert.Equal("The Limit field is required.", result.ErrorMessage);
        Assert.Equal(nameof(RequiredOrEmptyModel.Limit), Assert.Single(result.MemberNames));
    }

    [Fact]
    public void UnmetConditionRequiresEmpty()
    {
        // Arrange
        var model = new RequiredOrEmptyModel { UseLimit = false, Limit = 10 };

        // Act
        var results = ValidationTestHelper.Validate(model);

        // Assert
        var result = Assert.Single(results);
        Assert.Equal("The Limit field must be empty.", result.ErrorMessage);
        Assert.Equal(nameof(RequiredOrEmptyModel.Limit), Assert.Single(result.MemberNames));
    }

    [Fact]
    public void ErrorMessageCanBeChanged()
    {
        // Arrange
        var model = new EmptyWhenMessageModel { UseLimit = false, Limit = 10 };

        // Act
        var results = ValidationTestHelper.Validate(model);

        // Assert
        var result = Assert.Single(results);
        Assert.Equal("Limit is not allowed.", result.ErrorMessage);
    }

    [Fact]
    public void WhiteSpaceIsEmpty()
    {
        // Arrange
        var model = new EmptyWhenStringModel { Clear = true, Name = " ", Note = " " };

        // Act
        var results = ValidationTestHelper.Validate(model);

        // Assert
        var result = Assert.Single(results);
        Assert.Equal(nameof(EmptyWhenStringModel.Note), Assert.Single(result.MemberNames));
    }
}
