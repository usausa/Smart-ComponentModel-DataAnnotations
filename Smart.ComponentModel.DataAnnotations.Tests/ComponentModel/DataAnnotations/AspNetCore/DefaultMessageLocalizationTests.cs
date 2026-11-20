namespace Smart.ComponentModel.DataAnnotations.AspNetCore;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

public sealed class LocalizationForm
{
    [Required]
    public string? Title { get; set; }

    [Display(Name = "Start")]
    public DateOnly? From { get; set; }

    [CompareTo(CompareToOperation.GreaterThanOrEqual, nameof(From))]
    public DateOnly? To { get; set; }

    [Required(ErrorMessageResourceType = typeof(LocalizationResources), ErrorMessageResourceName = nameof(LocalizationResources.Required))]
    public string? Code { get; set; }

    [Range(1, 10)]
    public int Count { get; set; } = 1;
}

[AnyRequired(nameof(Phone), nameof(Mail))]
public sealed class LocalizationContact
{
    [Display(Name = "Phone number")]
    public string? Phone { get; set; }

    public string? Mail { get; set; }
}

public sealed class LocalizationParent
{
    public LocalizationContact? Contact { get; set; }
}

public static class LocalizationResources
{
    public static string Required => "{0} is required from resource.";
}

public sealed class DefaultMessageLocalizationTests
{
    private static readonly Dictionary<string, string> Messages = new()
    {
        ["RequiredAttribute_Error"] = "{0}を入力してください",
        ["CompareToAttribute_Error"] = "{0}は{1}以降にしてください",
        ["RangeAttribute_Error"] = "{0}は{1}から{2}の間にしてください",
        ["AnyRequiredAttribute_Error"] = "{1}のいずれかを入力してください",
        ["ValueMustBeANumber_Error"] = "{0}は数値で入力してください",
        ["AttemptedValueIsInvalid_Error"] = "{0}は{1}として正しくありません",
        ["Start"] = "開始日",
        ["Phone number"] = "電話番号"
    };

    // ------------------------------------------------------------
    // Attribute
    // ------------------------------------------------------------

    [Fact]
    public void DefaultMessageIsLocalized()
    {
        // Arrange
        using var provider = MvcTestHelper.CreateServices(Messages);

        // Act
        var modelState = MvcTestHelper.Validate(provider, new LocalizationForm { Code = "A" });

        // Assert
        Assert.Equal("Titleを入力してください", MvcTestHelper.GetError(modelState, nameof(LocalizationForm.Title)));
    }

    [Fact]
    public void MessageArgumentsAreFormatted()
    {
        // Arrange
        using var provider = MvcTestHelper.CreateServices(Messages);

        // Act
        var modelState = MvcTestHelper.Validate(provider, new LocalizationForm { Title = "A", Code = "A", Count = 20 });

        // Assert
        Assert.Equal("Countは1から10の間にしてください", MvcTestHelper.GetError(modelState, nameof(LocalizationForm.Count)));
    }

    [Fact]
    public void OtherDisplayNameIsLocalized()
    {
        // Arrange
        using var provider = MvcTestHelper.CreateServices(Messages);

        // Act
        var modelState = MvcTestHelper.Validate(provider, new LocalizationForm { Title = "A", Code = "A", From = new DateOnly(2026, 1, 2), To = new DateOnly(2026, 1, 1) });

        // Assert
        Assert.Equal("Toは開始日以降にしてください", MvcTestHelper.GetError(modelState, nameof(LocalizationForm.To)));
    }

    [Fact]
    public void TypeLevelAttributeUsesDisplayNames()
    {
        // Arrange
        using var provider = MvcTestHelper.CreateServices(Messages);

        // Act
        var modelState = MvcTestHelper.Validate(provider, new LocalizationContact());

        // Assert
        Assert.Equal("電話番号, Mailのいずれかを入力してください", MvcTestHelper.GetError(modelState, nameof(LocalizationContact.Phone)));
        Assert.Equal("電話番号, Mailのいずれかを入力してください", MvcTestHelper.GetError(modelState, nameof(LocalizationContact.Mail)));
    }

    [Fact]
    public void TypeLevelAttributeOfParameterUsesDisplayNames()
    {
        // Arrange
        using var provider = MvcTestHelper.CreateServices(Messages);
        var parameter = typeof(Action<LocalizationContact>).GetMethod(nameof(Action.Invoke))!.GetParameters()[0];

        // Act
        var modelState = MvcTestHelper.ValidateParameter(provider, new LocalizationContact(), parameter);

        // Assert
        Assert.Equal("電話番号, Mailのいずれかを入力してください", MvcTestHelper.GetError(modelState, nameof(LocalizationContact.Phone)));
    }

    [Fact]
    public void TypeLevelAttributeOfNestedPropertyUsesDisplayNames()
    {
        // Arrange
        using var provider = MvcTestHelper.CreateServices(Messages);

        // Act
        var modelState = MvcTestHelper.Validate(provider, new LocalizationParent { Contact = new LocalizationContact() });

        // Assert
        Assert.Equal("電話番号, Mailのいずれかを入力してください", MvcTestHelper.GetError(modelState, "Contact.Phone"));
    }

    [Fact]
    public void MemberAndTypeKeysTakePrecedence()
    {
        // Arrange
        using var provider = MvcTestHelper.CreateServices(new Dictionary<string, string>
        {
            ["RequiredAttribute_Error"] = "{0}を入力してください",
            ["LocalizationForm_RequiredAttribute_Error"] = "{0}は必須です",
            ["LocalizationForm_Title_RequiredAttribute_Error"] = "題名は必須です"
        });

        // Act
        var modelState = MvcTestHelper.Validate(provider, new LocalizationForm());

        // Assert
        Assert.Equal("題名は必須です", MvcTestHelper.GetError(modelState, nameof(LocalizationForm.Title)));
    }

    [Fact]
    public void ResourceMessageIsNotLocalized()
    {
        // Arrange
        using var provider = MvcTestHelper.CreateServices(Messages);

        // Act
        var modelState = MvcTestHelper.Validate(provider, new LocalizationForm { Title = "A" });

        // Assert
        Assert.Equal("Code is required from resource.", MvcTestHelper.GetError(modelState, nameof(LocalizationForm.Code)));
    }

    [Fact]
    public void MissingKeyKeepsDefaultMessage()
    {
        // Arrange
        using var provider = MvcTestHelper.CreateServices(new Dictionary<string, string>());

        // Act
        var modelState = MvcTestHelper.Validate(provider, new LocalizationForm { Code = "A" });

        // Assert
        Assert.Equal("The Title field is required.", MvcTestHelper.GetError(modelState, nameof(LocalizationForm.Title)));
    }

    // ------------------------------------------------------------
    // Model binding
    // ------------------------------------------------------------

    [Fact]
    public void ModelBindingMessageIsLocalized()
    {
        // Arrange
        using var provider = MvcTestHelper.CreateServices(Messages);

        // Act
        var messageProvider = provider.GetRequiredService<IOptions<MvcOptions>>().Value.ModelBindingMessageProvider;

        // Assert
        Assert.Equal("Countは数値で入力してください", messageProvider.ValueMustBeANumberAccessor("Count"));
        Assert.Equal("xは日付として正しくありません", messageProvider.AttemptedValueIsInvalidAccessor("x", "日付"));
    }

    [Fact]
    public void ModelBindingMessageFallsBackToDefault()
    {
        // Arrange
        using var provider = MvcTestHelper.CreateServices(new Dictionary<string, string>());

        // Act
        var messageProvider = provider.GetRequiredService<IOptions<MvcOptions>>().Value.ModelBindingMessageProvider;

        // Assert
        Assert.Equal("The field Count must be a number.", messageProvider.ValueMustBeANumberAccessor("Count"));
    }
}
