namespace Smart.ComponentModel.DataAnnotations.Generator.Models;

internal sealed record ComparisonModel(
    string TypeName,
    string Member,
    ValueKind MemberKind,
    string Other,
    ValueKind OtherKind,
    string ComparedTypeName);
