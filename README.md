# Smart.ComponentModel.DataAnnotations .NET - data annotations library for .NET

| Package | Info | Description |
|:-|:-|:-|
| Usa.Smart.ComponentModel.DataAnnotations | [![NuGet](https://img.shields.io/nuget/v/Usa.Smart.ComponentModel.DataAnnotations.svg)](https://www.nuget.org/packages/Usa.Smart.ComponentModel.DataAnnotations) | Conditional, comparison and collection attributes |
| Usa.Smart.ComponentModel.DataAnnotations.Japanese | [![NuGet](https://img.shields.io/nuget/v/Usa.Smart.ComponentModel.DataAnnotations.Japanese.svg)](https://www.nuget.org/packages/Usa.Smart.ComponentModel.DataAnnotations.Japanese) | Shift_JIS length and character type attributes |
| Usa.Smart.ComponentModel.DataAnnotations.AspNetCore | [![NuGet](https://img.shields.io/nuget/v/Usa.Smart.ComponentModel.DataAnnotations.AspNetCore.svg)](https://www.nuget.org/packages/Usa.Smart.ComponentModel.DataAnnotations.AspNetCore) | ASP.NET Core MVC client validation and message localization |

## What is this?

Validation attributes compatible with `System.ComponentModel.DataAnnotations`.
Attributes that read other members use code generated at compile time, so they work with Native AOT.

## Conditional

```csharp
public sealed class Form
{
    public bool UsePeriod { get; set; }

    [RequiredWhen(nameof(UsePeriod))]
    public DateOnly? From { get; set; }

    [RequiredWhen(nameof(UsePeriod))]
    [EmptyWhen(nameof(UsePeriod), ConditionValue = false)]
    [CompareToWhen(nameof(UsePeriod), CompareToOperation.GreaterThanOrEqual, nameof(From))]
    public DateOnly? To { get; set; }
}
```

## Comparison

```csharp
public sealed class Range
{
    public int Minimum { get; set; }

    [CompareTo(CompareToOperation.GreaterThanOrEqual, nameof(Minimum))]
    public int Maximum { get; set; }
}

public sealed class Account
{
    public string? Password { get; set; }

    [EqualTo(nameof(Password))]
    public string? ConfirmPassword { get; set; }
}
```

`[EqualTo]` checks equality in the same way as the standard `[Compare]`, and works with Native AOT.

## Any required

```csharp
[AnyRequired(nameof(Phone), nameof(Mail))]
public sealed class Contact
{
    public string? Phone { get; set; }

    public string? Mail { get; set; }
}
```

## Collection

```csharp
public sealed class Order
{
    [ElementRequired]
    [MaxCount(10)]
    [DuplicateKey(nameof(OrderItem.Code))]
    public List<OrderItem>? Items { get; set; }
}
```

## Japanese

```csharp
public sealed class Customer
{
    [Ms932MaxLength(40)]
    [Ms932Encodable]
    [FullWidth]
    public string? Name { get; set; }

    [Katakana(AllowSpace = true)]
    public string? Kana { get; set; }

    [NumericHyphen]
    public string? Phone { get; set; }
}
```

| Attribute | Description |
|:-|:-|
| `Ms932MaxLength` / `Ms932Length` / `Ms932FixedLength` | Byte length in Shift_JIS |
| `Ms932Encodable` | Only characters that can be encoded in Shift_JIS |
| `FullWidth` | Only 2-byte characters in Shift_JIS |
| `NoHalfWidthKana` | No half-width katakana |
| `Katakana` / `Hiragana` | Only full-width katakana / hiragana and the long vowel mark |
| `UpperAlphanumeric` / `UpperAlphanumericHyphen` / `Alphanumeric` / `AlphanumericHyphen` / `Numeric` / `NumericHyphen` | Only half-width letters, digits and hyphens |

## Source generator

Functions that read the members referenced by the attributes, their display names, and the comparers for `[CompareTo]` are generated from the attribute usages in the project that references the package directly.
The attributes are applied to properties or record primary constructor parameters, and the diagnostics are listed in [Diagnostics.md](Diagnostics.md).

## Localization

Default messages are English, and are localized by `IStringLocalizer` with the attribute type name as the key, the same as `Microsoft.Extensions.Validation`.

```csharp
["RequiredWhenAttribute_Error"] = "{0} is required.",
["CompareToAttribute_Error"] = "{0} must be greater than or equal to {1}.",
["Ms932MaxLengthAttribute_Error"] = "{0} must be at most {1} bytes."
```

## ASP.NET Core MVC

```csharp
builder.Services.AddControllersWithViews()
    .AddDataAnnotationsLocalization(static options => options.DataAnnotationLocalizerProvider = static (_, factory) => factory.Create(typeof(SharedResource)))
    .AddDefaultMessageLocalization()
    .AddValidationAttributeAdapters();
```

```html
<script src="~/lib/jquery-validation/jquery.validate.min.js"></script>
<script src="~/lib/jquery-validation-unobtrusive/jquery.validate.unobtrusive.min.js"></script>
<script src="~/_content/Usa.Smart.ComponentModel.DataAnnotations.AspNetCore/Smart.ComponentModel.DataAnnotations.AspNetCore.js"></script>
```

| Method | Description |
|:-|:-|
| `AddValidationAttributeAdapters()` | Outputs `data-val-*` for the attributes in this library |
| `AddDefaultMessageLocalization()` | Localizes default messages with the keys above, and model binding messages with `{Name}_Error` keys such as `ValueMustBeANumber_Error` (.NET 11) |

| Attribute | Client validation |
|:-|:-|
| `CompareTo` / `CompareToWhen` | Numbers and dates |
| `EqualTo` | Standard `equalto` rule |
| `RequiredWhen` / `EmptyWhen` | Yes |
| `Ms932MaxLength` / `Ms932Length` / `Ms932FixedLength` | Counts ASCII and half-width katakana as 1 byte, and other characters as 2 bytes |
| Character type attributes | Regular expression |
| `AnyRequired` / `AnyRequiredWhen` / `ElementRequired` / `MaxCount` / `DuplicateKey` / `Ms932Encodable` | Server only |
