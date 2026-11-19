namespace Smart.ComponentModel.DataAnnotations.Generator.Models;

internal sealed record DuplicateKeyModel(
    string Member,
    ValueKind Kind,
    string KeyTypeName,
    string? ItemTypeName,
    ValueKind ItemKind);
