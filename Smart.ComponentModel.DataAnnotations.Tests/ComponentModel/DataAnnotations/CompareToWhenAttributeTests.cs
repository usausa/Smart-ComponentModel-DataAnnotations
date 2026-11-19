namespace Smart.ComponentModel.DataAnnotations;

public sealed class CompareToWhenModel
{
    public bool UsePeriod { get; set; }

    public DateOnly? From { get; set; }

    [CompareToWhen(nameof(UsePeriod), CompareToOperation.GreaterThanOrEqual, nameof(From))]
    public DateOnly? To { get; set; }
}

public sealed class CompareToUnlessModel
{
    public bool? Open { get; set; }

    public int Minimum { get; set; }

    [CompareToWhen(nameof(Open), CompareToOperation.GreaterThan, nameof(Minimum), ConditionValue = false)]
    public int Maximum { get; set; }
}

public sealed class CompareToWhenAttributeTests
{
    [Fact]
    public void UnmetConditionSkipsComparison()
    {
        // Arrange
        var model = new CompareToWhenModel { UsePeriod = false, From = new DateOnly(2026, 2, 1), To = new DateOnly(2026, 1, 1) };

        // Act
        var results = ValidationTestHelper.Validate(model);

        // Assert
        Assert.Empty(results);
    }

    [Fact]
    public void MetConditionComparesWithOtherProperty()
    {
        // Arrange
        var model = new CompareToWhenModel { UsePeriod = true, From = new DateOnly(2026, 2, 1), To = new DateOnly(2026, 1, 1) };

        // Act
        var results = ValidationTestHelper.Validate(model);

        // Assert
        var result = Assert.Single(results);
        Assert.Equal("The field To must be greater than or equal to From.", result.ErrorMessage);
        Assert.Equal(nameof(CompareToWhenModel.To), Assert.Single(result.MemberNames));
    }

    [Fact]
    public void MetConditionWithValidOrderIsValid()
    {
        // Arrange
        var model = new CompareToWhenModel { UsePeriod = true, From = new DateOnly(2026, 1, 1), To = new DateOnly(2026, 1, 1) };

        // Act
        var results = ValidationTestHelper.Validate(model);

        // Assert
        Assert.Empty(results);
    }

    [Theory]
    [InlineData(false, 1)]
    [InlineData(true, 0)]
    [InlineData(null, 0)]
    public void ConditionValueSelectsCondition(bool? open, int expected)
    {
        // Arrange
        var model = new CompareToUnlessModel { Open = open, Minimum = 5, Maximum = 5 };

        // Act
        var results = ValidationTestHelper.Validate(model);

        // Assert
        Assert.Equal(expected, results.Count);
    }
}
