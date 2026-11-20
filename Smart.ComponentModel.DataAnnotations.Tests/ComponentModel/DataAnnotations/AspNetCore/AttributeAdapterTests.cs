namespace Smart.ComponentModel.DataAnnotations.AspNetCore;

public sealed class AdapterForm
{
    public bool UsePeriod { get; set; }

    [Display(Name = "Start")]
    public DateOnly? From { get; set; }

    [CompareTo(CompareToOperation.GreaterThanOrEqual, nameof(From))]
    public DateOnly? To { get; set; }

    public int? Minimum { get; set; }

    [CompareToWhen(nameof(UsePeriod), CompareToOperation.GreaterThan, nameof(Minimum))]
    public int? Maximum { get; set; }

    public string? Code { get; set; }

    [CompareTo(CompareToOperation.GreaterThan, nameof(Code))]
    public string? NextCode { get; set; }

    [Display(Name = "Mail address")]
    public string? Mail { get; set; }

    [EqualTo(nameof(Mail))]
    public string? ConfirmMail { get; set; }

    [RequiredWhen(nameof(UsePeriod), AllowEmptyStrings = true)]
    public string? Note { get; set; }

    [EmptyWhen(nameof(UsePeriod), ConditionValue = false)]
    public string? Reason { get; set; }

    [Required]
    public string? Title { get; set; }

    [Ms932MaxLength(20)]
    public string? Name { get; set; }

    [Ms932Length(2, 10)]
    public string? Kana { get; set; }

    [Ms932FixedLength(4)]
    public string? Fixed { get; set; }

    [UpperAlphanumeric]
    public string? Id { get; set; }

    [ElementRequired]
    public IReadOnlyList<string?>? Items { get; set; }
}

public sealed class AttributeAdapterTests
{
    // ------------------------------------------------------------
    // Rule
    // ------------------------------------------------------------

    [Fact]
    public void CompareToOutputsRule()
    {
        // Arrange
        using var provider = MvcTestHelper.CreateServices();

        // Act
        var attributes = MvcTestHelper.AddValidation(provider, typeof(AdapterForm), nameof(AdapterForm.To));

        // Assert
        Assert.Equal("The field To must be greater than or equal to Start.", attributes["data-val-compareto"]);
        Assert.Equal("*.From", attributes["data-val-compareto-other"]);
        Assert.Equal("GreaterThanOrEqual", attributes["data-val-compareto-operation"]);
        Assert.Equal("date", attributes["data-val-compareto-type"]);
    }

    [Fact]
    public void CompareToOfStringHasNoRule()
    {
        // Arrange
        using var provider = MvcTestHelper.CreateServices();

        // Act
        var attributes = MvcTestHelper.AddValidation(provider, typeof(AdapterForm), nameof(AdapterForm.NextCode));

        // Assert
        Assert.Empty(attributes);
    }

    [Fact]
    public void CompareToWhenOutputsRule()
    {
        // Arrange
        using var provider = MvcTestHelper.CreateServices();

        // Act
        var attributes = MvcTestHelper.AddValidation(provider, typeof(AdapterForm), nameof(AdapterForm.Maximum));

        // Assert
        Assert.Equal("*.UsePeriod", attributes["data-val-comparetowhen-condition"]);
        Assert.Equal("true", attributes["data-val-comparetowhen-value"]);
        Assert.Equal("*.Minimum", attributes["data-val-comparetowhen-other"]);
        Assert.Equal("GreaterThan", attributes["data-val-comparetowhen-operation"]);
        Assert.Equal("number", attributes["data-val-comparetowhen-type"]);
    }

    [Fact]
    public void EqualToOutputsStandardRule()
    {
        // Arrange
        using var provider = MvcTestHelper.CreateServices();

        // Act
        var attributes = MvcTestHelper.AddValidation(provider, typeof(AdapterForm), nameof(AdapterForm.ConfirmMail));

        // Assert
        Assert.Equal("The field ConfirmMail must match Mail address.", attributes["data-val-equalto"]);
        Assert.Equal("*.Mail", attributes["data-val-equalto-other"]);
    }

    [Fact]
    public void RequiredWhenOutputsRule()
    {
        // Arrange
        using var provider = MvcTestHelper.CreateServices();

        // Act
        var attributes = MvcTestHelper.AddValidation(provider, typeof(AdapterForm), nameof(AdapterForm.Note));

        // Assert
        Assert.Equal("The Note field is required.", attributes["data-val-requiredwhen"]);
        Assert.Equal("*.UsePeriod", attributes["data-val-requiredwhen-condition"]);
        Assert.Equal("true", attributes["data-val-requiredwhen-value"]);
        Assert.Equal("true", attributes["data-val-requiredwhen-allowempty"]);
    }

    [Fact]
    public void EmptyWhenOutputsRule()
    {
        // Arrange
        using var provider = MvcTestHelper.CreateServices();

        // Act
        var attributes = MvcTestHelper.AddValidation(provider, typeof(AdapterForm), nameof(AdapterForm.Reason));

        // Assert
        Assert.Equal("The Reason field must be empty.", attributes["data-val-emptywhen"]);
        Assert.Equal("false", attributes["data-val-emptywhen-value"]);
        Assert.Equal("false", attributes["data-val-emptywhen-allowempty"]);
    }

    [Theory]
    [InlineData(nameof(AdapterForm.Name), "0", "20")]
    [InlineData(nameof(AdapterForm.Kana), "2", "10")]
    [InlineData(nameof(AdapterForm.Fixed), "4", "4")]
    public void Ms932LengthOutputsRange(string property, string minimum, string maximum)
    {
        // Arrange
        using var provider = MvcTestHelper.CreateServices();

        // Act
        var attributes = MvcTestHelper.AddValidation(provider, typeof(AdapterForm), property);

        // Assert
        Assert.Equal(minimum, attributes["data-val-ms932length-min"]);
        Assert.Equal(maximum, attributes["data-val-ms932length-max"]);
    }

    [Fact]
    public void Ms932LengthUsesDefaultMessage()
    {
        // Arrange
        using var provider = MvcTestHelper.CreateServices();

        // Act
        var attributes = MvcTestHelper.AddValidation(provider, typeof(AdapterForm), nameof(AdapterForm.Name));

        // Assert
        Assert.Equal(new Ms932MaxLengthAttribute(20).FormatErrorMessage(nameof(AdapterForm.Name)), attributes["data-val-ms932length"]);
    }

    [Fact]
    public void CharacterPatternOutputsPattern()
    {
        // Arrange
        using var provider = MvcTestHelper.CreateServices();

        // Act
        var attributes = MvcTestHelper.AddValidation(provider, typeof(AdapterForm), nameof(AdapterForm.Id));

        // Assert
        Assert.Equal(new UpperAlphanumericAttribute().Pattern, attributes["data-val-charactertype-pattern"]);
    }

    [Fact]
    public void ServerOnlyAttributeHasNoRule()
    {
        // Arrange
        using var provider = MvcTestHelper.CreateServices();

        // Act
        var attributes = MvcTestHelper.AddValidation(provider, typeof(AdapterForm), nameof(AdapterForm.Items));

        // Assert
        Assert.Empty(attributes);
    }

    [Fact]
    public void StandardAttributeIsUnchangedWithoutLocalization()
    {
        // Arrange
        using var provider = MvcTestHelper.CreateServices();

        // Act
        var attributes = MvcTestHelper.AddValidation(provider, typeof(AdapterForm), nameof(AdapterForm.Title));

        // Assert
        Assert.Equal("The Title field is required.", attributes["data-val-required"]);
    }

    // ------------------------------------------------------------
    // Localization
    // ------------------------------------------------------------

    [Fact]
    public void MessageUsesLocalizedTemplateAndDisplayName()
    {
        // Arrange
        using var provider = MvcTestHelper.CreateServices(new Dictionary<string, string>
        {
            ["CompareToAttribute_Error"] = "{0}は{1}以降にしてください",
            ["Start"] = "開始日"
        });

        // Act
        var attributes = MvcTestHelper.AddValidation(provider, typeof(AdapterForm), nameof(AdapterForm.To));

        // Assert
        Assert.Equal("Toは開始日以降にしてください", attributes["data-val-compareto"]);
    }

    [Fact]
    public void StandardAttributeMessageIsLocalized()
    {
        // Arrange
        using var provider = MvcTestHelper.CreateServices(new Dictionary<string, string>
        {
            ["RequiredAttribute_Error"] = "{0}を入力してください"
        });

        // Act
        var attributes = MvcTestHelper.AddValidation(provider, typeof(AdapterForm), nameof(AdapterForm.Title));

        // Assert
        Assert.Equal("Titleを入力してください", attributes["data-val-required"]);
    }

    [Fact]
    public void MemberKeyTakesPrecedence()
    {
        // Arrange
        using var provider = MvcTestHelper.CreateServices(new Dictionary<string, string>
        {
            ["Ms932MaxLengthAttribute_Error"] = "{0}は{1}バイト以内",
            ["AdapterForm_Ms932MaxLengthAttribute_Error"] = "{0}は{1}バイトまで",
            ["AdapterForm_Name_Ms932MaxLengthAttribute_Error"] = "名前は{1}バイトまで"
        });

        // Act
        var attributes = MvcTestHelper.AddValidation(provider, typeof(AdapterForm), nameof(AdapterForm.Name));

        // Assert
        Assert.Equal("名前は20バイトまで", attributes["data-val-ms932length"]);
    }

    [Fact]
    public void TypeKeyTakesPrecedenceOverGlobalKey()
    {
        // Arrange
        using var provider = MvcTestHelper.CreateServices(new Dictionary<string, string>
        {
            ["Ms932LengthAttribute_Error"] = "{0}は{1}から{2}バイト",
            ["AdapterForm_Ms932LengthAttribute_Error"] = "{0}は{1}〜{2}バイト"
        });

        // Act
        var attributes = MvcTestHelper.AddValidation(provider, typeof(AdapterForm), nameof(AdapterForm.Kana));

        // Assert
        Assert.Equal("Kanaは2〜10バイト", attributes["data-val-ms932length"]);
    }
}
