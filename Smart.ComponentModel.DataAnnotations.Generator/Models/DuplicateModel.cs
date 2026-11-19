namespace Smart.ComponentModel.DataAnnotations.Generator.Models;

using SourceGenerateHelper;

internal sealed record DuplicateModel(
    string TypeName,
    string Member,
    string Keys,
    ValueKind CollectionKind,
    string ElementTypeName,
    ValueKind ElementKind,
    EquatableArray<DuplicateKeyModel> KeyMembers);
