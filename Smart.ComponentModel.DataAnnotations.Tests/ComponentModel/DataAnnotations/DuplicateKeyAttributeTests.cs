namespace Smart.ComponentModel.DataAnnotations;

internal sealed class DuplicateKeyEntry
{
    public string? Code { get; set; }

    public int Branch { get; set; }

    public int? Group { get; set; }

    public IReadOnlyList<string?>? Tags { get; set; }
}

internal struct DuplicateKeyValue
{
    public int Code { get; set; }
}

internal sealed class DuplicateKeyModel
{
    [DuplicateKey(nameof(DuplicateKeyEntry.Code))]
    public List<DuplicateKeyEntry?>? Entries { get; set; }

    [DuplicateKey(nameof(DuplicateKeyEntry.Code), nameof(DuplicateKeyEntry.Branch))]
    [DuplicateKey(nameof(DuplicateKeyEntry.Group))]
    public DuplicateKeyEntry[]? Composites { get; set; }

    [DuplicateKey(nameof(DuplicateKeyEntry.Tags))]
    public IEnumerable<DuplicateKeyEntry>? Tagged { get; set; }

    [DuplicateKey(nameof(DuplicateKeyValue.Code))]
    public List<DuplicateKeyValue>? Values { get; set; }
}

public sealed class DuplicateKeyAttributeTests
{
    [Fact]
    public void UniqueKeysAreValid()
    {
        // Arrange
        var model = new DuplicateKeyModel { Entries = [new DuplicateKeyEntry { Code = "A" }, new DuplicateKeyEntry { Code = "B" }, new DuplicateKeyEntry(), new DuplicateKeyEntry(), null] };

        // Act
        var results = ValidationTestHelper.Validate(model);

        // Assert
        Assert.Empty(results);
    }

    [Fact]
    public void DuplicateKeysAreInvalid()
    {
        // Arrange
        var model = new DuplicateKeyModel { Entries = [new DuplicateKeyEntry { Code = "A" }, new DuplicateKeyEntry { Code = "A" }] };

        // Act
        var results = ValidationTestHelper.Validate(model);

        // Assert
        var result = Assert.Single(results);
        Assert.Equal("The field Entries must not contain duplicate Code.", result.ErrorMessage);
        Assert.Equal(nameof(DuplicateKeyModel.Entries), Assert.Single(result.MemberNames));
    }

    [Fact]
    public void CompositeKeysAreCompared()
    {
        // Arrange
        var unique = new DuplicateKeyModel { Composites = [new DuplicateKeyEntry { Code = "A", Branch = 1 }, new DuplicateKeyEntry { Code = "A", Branch = 2 }] };
        var duplicate = new DuplicateKeyModel { Composites = [new DuplicateKeyEntry { Code = "A", Branch = 1 }, new DuplicateKeyEntry { Code = "A", Branch = 1 }] };

        // Act
        var valid = ValidationTestHelper.Validate(unique);
        var invalid = ValidationTestHelper.Validate(duplicate);

        // Assert
        Assert.Empty(valid);
        var result = Assert.Single(invalid);
        Assert.Equal("The field Composites must not contain duplicate Code, Branch.", result.ErrorMessage);
    }

    [Fact]
    public void NullableValueKeysSkipNull()
    {
        // Arrange
        var model = new DuplicateKeyModel { Composites = [new DuplicateKeyEntry { Code = "A", Branch = 1 }, new DuplicateKeyEntry { Code = "B", Branch = 1 }, new DuplicateKeyEntry { Code = "C", Group = 1 }, new DuplicateKeyEntry { Code = "D", Group = 1 }] };

        // Act
        var results = ValidationTestHelper.Validate(model);

        // Assert
        var result = Assert.Single(results);
        Assert.Equal("The field Composites must not contain duplicate Group.", result.ErrorMessage);
    }

    [Fact]
    public void ElementsOfListKeyAreCompared()
    {
        // Arrange
        var unique = new DuplicateKeyModel { Tagged = [new DuplicateKeyEntry { Tags = ["a", "b", null] }, new DuplicateKeyEntry { Tags = ["c", null] }, new DuplicateKeyEntry()] };
        var duplicate = new DuplicateKeyModel { Tagged = [new DuplicateKeyEntry { Tags = ["a", "b"] }, new DuplicateKeyEntry { Tags = ["b"] }] };

        // Act
        var valid = ValidationTestHelper.Validate(unique);
        var invalid = ValidationTestHelper.Validate(duplicate);

        // Assert
        Assert.Empty(valid);
        Assert.Single(invalid);
    }

    [Fact]
    public void ValueTypeElementsAreCompared()
    {
        // Arrange
        var model = new DuplicateKeyModel { Values = [new DuplicateKeyValue { Code = 1 }, new DuplicateKeyValue { Code = 1 }] };

        // Act
        var results = ValidationTestHelper.Validate(model);

        // Assert
        Assert.Single(results);
    }

    [Fact]
    public void NullCollectionIsValid()
    {
        // Arrange
        var model = new DuplicateKeyModel();

        // Act
        var results = ValidationTestHelper.Validate(model);

        // Assert
        Assert.Empty(results);
    }

    [Fact]
    public void FormatErrorMessageUsesMembers()
    {
        // Act
        var message = new DuplicateKeyAttribute(nameof(DuplicateKeyEntry.Code), nameof(DuplicateKeyEntry.Branch)).FormatErrorMessage("Entries");

        // Assert
        Assert.Equal("The field Entries must not contain duplicate Code, Branch.", message);
    }

    [Fact]
    public void UnregisteredKeysThrow()
    {
        // Arrange
        var attribute = new DuplicateKeyAttribute(nameof(DuplicateKeyEntry.Branch));
        var model = new DuplicateKeyModel { Entries = [new DuplicateKeyEntry { Code = "A" }] };

        // Act
        var exception = Record.Exception(() => ValidationTestHelper.ValidateProperty(attribute, model, nameof(DuplicateKeyModel.Entries), model.Entries));

        // Assert
        Assert.IsType<InvalidOperationException>(exception);
    }

    [Fact]
    public void EmptyMembersThrow()
    {
        // Arrange
        var attribute = new DuplicateKeyAttribute();
        var model = new DuplicateKeyModel { Entries = [] };

        // Act
        var exception = Record.Exception(() => ValidationTestHelper.ValidateProperty(attribute, model, nameof(DuplicateKeyModel.Entries), model.Entries));

        // Assert
        Assert.IsType<InvalidOperationException>(exception);
    }
}
