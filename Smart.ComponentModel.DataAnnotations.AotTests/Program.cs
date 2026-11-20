using System.ComponentModel.DataAnnotations;

using Smart.ComponentModel.DataAnnotations;
using Smart.ComponentModel.DataAnnotations.AotTests;

static void Assert(bool condition, string message)
{
    if (!condition)
    {
        Console.Error.WriteLine($"FAIL: {message}");
        Environment.Exit(1);
    }
}

static ValidationResult? Validate(ValidationAttribute attribute, object instance, string name, object? value) =>
    attribute.GetValidationResult(value, new ValidationContext(instance, name, null, null) { MemberName = name });

Console.WriteLine("Smart.ComponentModel.DataAnnotations AOT smoke tests starting...");

//--------------------------------------------------------------------------------
// 1. CompareTo
//--------------------------------------------------------------------------------
{
    var form = new Form { From = new DateOnly(2026, 1, 2), To = new DateOnly(2026, 1, 1) };
    var attribute = new CompareToAttribute(CompareToOperation.GreaterThanOrEqual, nameof(Form.From));

    var invalid = Validate(attribute, form, nameof(Form.To), form.To);
    Assert(invalid is not null, "CompareTo invalid");
    Assert(invalid!.ErrorMessage == "The field To must be greater than or equal to Start.", $"CompareTo message: {invalid.ErrorMessage}");

    form.To = new DateOnly(2026, 1, 3);
    Assert(Validate(attribute, form, nameof(Form.To), form.To) is null, "CompareTo valid");
    Console.WriteLine("  [OK] CompareTo");
}

//--------------------------------------------------------------------------------
// 2. RequiredWhen / EmptyWhen
//--------------------------------------------------------------------------------
{
    var form = new Form { UsePeriod = true, Days = 3 };
    var required = new RequiredWhenAttribute(nameof(Form.UsePeriod));
    var empty = new EmptyWhenAttribute(nameof(Form.UsePeriod)) { ConditionValue = false };

    Assert(Validate(required, form, nameof(Form.From), form.From) is not null, "RequiredWhen invalid");
    Assert(Validate(empty, form, nameof(Form.Days), form.Days) is null, "EmptyWhen valid");

    form.UsePeriod = false;
    Assert(Validate(required, form, nameof(Form.From), form.From) is null, "RequiredWhen valid");
    Assert(Validate(empty, form, nameof(Form.Days), form.Days) is not null, "EmptyWhen invalid");
    Console.WriteLine("  [OK] RequiredWhen / EmptyWhen");
}

//--------------------------------------------------------------------------------
// 3. AnyRequired
//--------------------------------------------------------------------------------
{
    var form = new Form();
    var attribute = new AnyRequiredAttribute(nameof(Form.Phone), nameof(Form.Mail));
    var context = new ValidationContext(form, nameof(Form), null, null);

    var invalid = attribute.GetValidationResult(form, context);
    Assert(invalid is not null, "AnyRequired invalid");
    Assert(invalid!.ErrorMessage == "At least one of the fields Phone, Mail address is required.", $"AnyRequired message: {invalid.ErrorMessage}");

    form.Mail = "user@example.com";
    Assert(attribute.GetValidationResult(form, context) is null, "AnyRequired valid");
    Console.WriteLine("  [OK] AnyRequired");
}

//--------------------------------------------------------------------------------
// 4. DuplicateKey
//--------------------------------------------------------------------------------
{
    var form = new Form { Entries = [new Entry { Code = "A", Branch = 1 }, new Entry { Code = "A", Branch = 2 }] };
    var single = new DuplicateKeyAttribute(nameof(Entry.Code));
    var composite = new DuplicateKeyAttribute(nameof(Entry.Code), nameof(Entry.Branch));

    Assert(Validate(single, form, nameof(Form.Entries), form.Entries) is not null, "DuplicateKey single invalid");
    Assert(Validate(composite, form, nameof(Form.Entries), form.Entries) is null, "DuplicateKey composite valid");
    Console.WriteLine("  [OK] DuplicateKey");
}

//--------------------------------------------------------------------------------
// 5. EqualTo / IComparable<T>
//--------------------------------------------------------------------------------
{
    var form = new Form { Mail = "a@example.com", ConfirmMail = "b@example.com", Minimum = new Level(2), Maximum = new Level(1) };
    var equal = new EqualToAttribute(nameof(Form.Mail));
    var compare = new CompareToAttribute(CompareToOperation.GreaterThanOrEqual, nameof(Form.Minimum));

    var invalid = Validate(equal, form, nameof(Form.ConfirmMail), form.ConfirmMail);
    Assert(invalid is not null, "EqualTo invalid");
    Assert(invalid!.ErrorMessage == "The field ConfirmMail must match Mail address.", $"EqualTo message: {invalid.ErrorMessage}");
    Assert(Validate(compare, form, nameof(Form.Maximum), form.Maximum) is not null, "CompareTo generic comparable invalid");

    form.ConfirmMail = form.Mail;
    form.Maximum = new Level(3);
    Assert(Validate(equal, form, nameof(Form.ConfirmMail), form.ConfirmMail) is null, "EqualTo valid");
    Assert(Validate(compare, form, nameof(Form.Maximum), form.Maximum) is null, "CompareTo generic comparable valid");
    Console.WriteLine("  [OK] EqualTo / IComparable<T>");
}

//--------------------------------------------------------------------------------
// 6. Japanese
//--------------------------------------------------------------------------------
{
    Assert(new Ms932MaxLengthAttribute(4).IsValid("あい"), "Ms932MaxLength valid");
    Assert(!new Ms932MaxLengthAttribute(4).IsValid("あいう"), "Ms932MaxLength invalid");
    Assert(!new Ms932EncodableAttribute().IsValid("〜"), "Ms932Encodable invalid");
    Assert(new FullWidthAttribute().IsValid("あＡ"), "FullWidth valid");
    Assert(!new FullWidthAttribute().IsValid("あA"), "FullWidth invalid");
    Assert(new KatakanaAttribute().IsValid("カー"), "Katakana valid");
    Assert(!new NumericAttribute().IsValid("１"), "Numeric invalid");
    Console.WriteLine("  [OK] Japanese");
}

Console.WriteLine("All AOT smoke tests passed.");
