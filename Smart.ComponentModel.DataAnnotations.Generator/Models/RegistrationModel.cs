namespace Smart.ComponentModel.DataAnnotations.Generator.Models;

using SourceGenerateHelper;

internal sealed record RegistrationModel(
    EquatableArray<ConditionModel> Conditions,
    EquatableArray<PresenceModel> Presences,
    EquatableArray<ComparisonModel> Comparisons,
    EquatableArray<DuplicateModel> Duplicates,
    EquatableArray<DisplayNameModel> DisplayNames)
{
    public static RegistrationModel Empty { get; } = new([], [], [], [], []);

    public bool IsEmpty =>
        (Conditions.Count == 0) && (Presences.Count == 0) && (Comparisons.Count == 0) && (Duplicates.Count == 0) && (DisplayNames.Count == 0);
}
